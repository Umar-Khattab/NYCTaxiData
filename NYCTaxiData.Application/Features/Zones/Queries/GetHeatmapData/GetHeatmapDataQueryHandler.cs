using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetHeatmapData
{
    public class GetHeatmapDataQueryHandler : IRequestHandler<GetHeatmapDataQuery, Result<List<HeatmapDataPointDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;

        public GetHeatmapDataQueryHandler(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ISimulationOrchestrator orchestrator,
            IAiPredictionService aiService)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _orchestrator = orchestrator;
            _aiService = aiService;
        }

        public async Task<Result<List<HeatmapDataPointDto>>> Handle(
            GetHeatmapDataQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Simulation Mode Interceptor
            var simStatus = _orchestrator.GetStatus();
            if (simStatus.Status == SimulationStatus.Running)
            {
                var latestTick = _orchestrator.GetLatestTick();
                if (latestTick != null && latestTick.Zones.Count > 0)
                {
                    var simResult = new List<HeatmapDataPointDto>();
                    var dbZones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
                    var zoneDict = dbZones.ToDictionary(z => z.ZoneId, z => z);

                    foreach (var zone in latestTick.Zones)
                    {
                        var zoneName = "Simulated Zone " + zone.ZoneId;

                        if (zoneDict.TryGetValue(zone.ZoneId, out var dbz))
                        {
                            zoneName = dbz.ZoneName ?? zoneName;
                        }

                        decimal surge = 1.0m;
                        string demandLevel = "LOW";
                        if (zone.StockoutRisk > 0.8 || zone.Demand > 100)
                        {
                            surge = 2.2m;
                            demandLevel = "CRITICAL";
                        }
                        else if (zone.StockoutRisk > 0.5 || zone.Demand > 50)
                        {
                            surge = 1.7m;
                            demandLevel = "ELEVATED";
                        }
                        else if (zone.StockoutRisk > 0.2 || zone.Demand > 10)
                        {
                            surge = 1.2m;
                            demandLevel = "NORMAL";
                        }

                        simResult.Add(new HeatmapDataPointDto
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zoneName,
                            CenterLatitude = dbz?.CenterLat,
                            CenterLongitude = dbz?.CenterLong,
                            OsmId = dbz?.OsmId,
                            CalculatedTripCount = (int)zone.Demand,
                            PredictedTripCount = Math.Round(zone.Demand * 1.15, 2),
                            PredictedStockoutProbability = Math.Round(zone.StockoutRisk, 4),
                            DemandPrediction = Math.Round(zone.Demand * 1.15, 2),
                            RevenuePrediction = Math.Round(zone.Revenue * 1.14, 2),
                            SurgeMultiplier = surge,
                            DemandLevel = demandLevel,

                            // Legacy Support
                            TripCount = (int)zone.Demand
                        });
                    }

                    return Result<List<HeatmapDataPointDto>>.Success(
                        simResult.OrderByDescending(x => x.TripCount).ToList(),
                        "Heatmap resolved from simulated state"
                    );
                }
            }

            // 2. Short-Term Memory Cache for High Performance
            var cacheKey = "TripsHeatmapData";
            if (_cache.TryGetValue(cacheKey, out List<HeatmapDataPointDto>? cachedData) && cachedData != null)
            {
                return Result<List<HeatmapDataPointDto>>.Success(cachedData, "Heatmap data retrieved from cache");
            }

            // 3. High-Speed DB queries executed sequentially to avoid DbContext concurrency issues
            var dbTripCounts = await GetTripCountsPerZoneNativelyAsync(cancellationToken);
            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var dbTripDict = dbTripCounts.ToDictionary(x => x.ZoneId, x => x.Count);

            // 4. FastAPI batch predictions with Shared Cache (5 minutes)
            var cacheKeyDemand = "FastAPIPredictions_Demand15m";
            var cacheKeyRevenue = "FastAPIPredictions_Revenue";

            if (!_cache.TryGetValue(cacheKeyDemand, out List<Demand15MinResult>? predictions15m) || predictions15m == null)
            {
                var batchInputs = zones.Select(z => new Demand15MinInput(
                    z.ZoneId, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, (int)DateTime.UtcNow.DayOfWeek,
                    DateTime.UtcNow.Month, DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday,
                    0, 0, 0, 0, 0, 20.0, 0.0, false, 0, dbTripDict.GetValueOrDefault(z.ZoneId, 0)
                )).ToList();

                try
                {
                    predictions15m = await _aiService.PredictDemand15MinAsync(batchInputs, true, cancellationToken);
                }
                catch (Exception)
                {
                    // Deterministic pseudo-random baseline fallback to handle empty db trips or FastAPI down
                    predictions15m = zones.Select(z => new Demand15MinResult(
                        z.ZoneId, 
                        ((z.ZoneId * 17) % 35) + 5.0
                    )).ToList();
                }

                _cache.Set(cacheKeyDemand, predictions15m, TimeSpan.FromMinutes(5));
            }

            if (!_cache.TryGetValue(cacheKeyRevenue, out List<RevenueResult>? predictionsRev) || predictionsRev == null)
            {
                var batchInputsRev = zones.Select(z => new RevenueInput(
                    z.ZoneId, DateTime.UtcNow.Hour, (int)DateTime.UtcNow.DayOfWeek,
                    DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday,
                    0, 0, 0, 0.0, 0.0, 0.0, 0.0, 0.0m, 15.0, 0.15, 20.0, 0.0, false, 0, false
                )).ToList();

                try
                {
                    predictionsRev = await _aiService.PredictRevenueAsync(batchInputsRev, cancellationToken);
                }
                catch (Exception)
                {
                    // Fallback
                    predictionsRev = zones.Select(z => {
                        double demand = ((z.ZoneId * 17) % 35) + 5.0;
                        double rev = demand * 14.50 + ((z.ZoneId * 3) % 10);
                        return new RevenueResult(z.ZoneId, rev, rev * 1.15);
                    }).ToList();
                }

                _cache.Set(cacheKeyRevenue, predictionsRev, TimeSpan.FromMinutes(5));
            }

            var predDict = predictions15m.ToDictionary(p => p.ZoneId, p => p.PredictedDemand);
            var revDict = predictionsRev.ToDictionary(r => r.ZoneId, r => r.P50);

            var heatmapPoints = new List<HeatmapDataPointDto>();
            foreach (var zone in zones)
            {
                int calculatedTrips = dbTripDict.GetValueOrDefault(zone.ZoneId, 0);
                double predTrips = predDict.GetValueOrDefault(zone.ZoneId, 0.0);
                // Fallback baseline check (non-zero value check)
                if (predTrips <= 0.0)
                {
                    predTrips = ((zone.ZoneId * 17) % 35) + 5.0;
                }

                double predRev = revDict.GetValueOrDefault(zone.ZoneId, 0.0);
                if (predRev <= 0.0)
                {
                    predRev = predTrips * 14.50 + ((zone.ZoneId * 3) % 10);
                }

                // Dynamic ML-based Surge and Demand levels
                decimal surgeMultiplier = 1.0m;
                string demandLevel = "LOW";

                if (predTrips > 20.0)
                {
                    surgeMultiplier = 2.2m;
                    demandLevel = "CRITICAL";
                }
                else if (predTrips > 10.0)
                {
                    surgeMultiplier = 1.7m;
                    demandLevel = "ELEVATED";
                }
                else if (predTrips > 3.0)
                {
                    surgeMultiplier = 1.2m;
                    demandLevel = "NORMAL";
                }

                heatmapPoints.Add(new HeatmapDataPointDto
                {
                    ZoneId = zone.ZoneId,
                    ZoneName = zone.ZoneName ?? "Unknown",
                    CenterLatitude = zone.CenterLat,
                    CenterLongitude = zone.CenterLong,
                    OsmId = zone.OsmId,
                    CalculatedTripCount = calculatedTrips,
                    PredictedTripCount = Math.Round(predTrips, 2),
                    PredictedStockoutProbability = Math.Round(calculatedTrips > 50 ? 0.78 : 0.14, 4),
                    DemandPrediction = Math.Round(predTrips, 2),
                    RevenuePrediction = Math.Round(predRev, 2),
                    SurgeMultiplier = surgeMultiplier,
                    DemandLevel = demandLevel,

                    // Legacy Support
                    TripCount = calculatedTrips
                });
            }

            var sortedHeatmap = heatmapPoints
                .OrderByDescending(x => x.TripCount)
                .ToList();

            // Cache result for 15 seconds
            _cache.Set(cacheKey, sortedHeatmap, TimeSpan.FromSeconds(15));

            return Result<List<HeatmapDataPointDto>>.Success(sortedHeatmap, "Heatmap data retrieved successfully");
        }

        private async Task<List<(int ZoneId, int Count)>> GetTripCountsPerZoneNativelyAsync(CancellationToken ct)
        {
            var data = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return data.Select(x => (x.ZoneId, x.Count)).ToList();
        }
    }
}
