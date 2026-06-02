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
                        var borough = "Manhattan";

                        if (zoneDict.TryGetValue(zone.ZoneId, out var dbz))
                        {
                            zoneName = dbz.ZoneName;
                            borough = dbz.Borough ?? "Unknown";
                        }

                        double baseLat = 40.7306;
                        double baseLon = -73.9352;
                        double latOffset = ((zone.ZoneId * 17) % 100) * 0.001 - 0.05;
                        double lonOffset = ((zone.ZoneId * 23) % 100) * 0.001 - 0.05;

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
                            Borough = borough,
                            Latitude = Math.Round(baseLat + latOffset, 4),
                            Longitude = Math.Round(baseLon + lonOffset, 4),
                            CalculatedTripCount = (int)zone.Demand,
                            PredictedTripCount = Math.Round(zone.Demand * 1.15, 2),
                            PredictedStockoutProbability = Math.Round(zone.StockoutRisk, 4),
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

            // 3. High-Speed Native SQL Execution
            var dbTripCounts = await GetTripCountsPerZoneNativelyAsync(cancellationToken);
            var dbTripDict = dbTripCounts.ToDictionary(x => x.ZoneId, x => x.Count);

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var heatmapPoints = new List<HeatmapDataPointDto>();

            // 4. FastAPI batch predictions
            var batchInputs = zones.Select(z => new Demand15MinInput(
                z.ZoneId, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, (int)DateTime.UtcNow.DayOfWeek,
                DateTime.UtcNow.Month, DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday,
                0, 0, 0, 0, 0, 20.0, 0.0, false, 0, dbTripDict.GetValueOrDefault(z.ZoneId, 0)
            )).ToList();

            List<Demand15MinResult> predictions15m = new();
            try
            {
                predictions15m = await _aiService.PredictDemand15MinAsync(batchInputs, true, cancellationToken);
            }
            catch (Exception)
            {
                // Resilient fallback heuristic if FastAPI fails
                predictions15m = zones.Select(z => new Demand15MinResult(z.ZoneId, dbTripDict.GetValueOrDefault(z.ZoneId, 0) / 4.0 * 1.1)).ToList();
            }

            var predDict = predictions15m.ToDictionary(p => p.ZoneId, p => p.PredictedDemand);

            foreach (var zone in zones)
            {
                int calculatedTrips = dbTripDict.GetValueOrDefault(zone.ZoneId, 0);
                double predTrips = predDict.GetValueOrDefault(zone.ZoneId, calculatedTrips / 4.0 * 1.1);

                double baseLat = 40.7306;
                double baseLon = -73.9352;
                double latOffset = ((zone.ZoneId * 17) % 100) * 0.001 - 0.05;
                double lonOffset = ((zone.ZoneId * 23) % 100) * 0.001 - 0.05;

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
                    ZoneName = zone.ZoneName,
                    Borough = zone.Borough ?? "Unknown",
                    Latitude = Math.Round(baseLat + latOffset, 4),
                    Longitude = Math.Round(baseLon + lonOffset, 4),
                    CalculatedTripCount = calculatedTrips,
                    PredictedTripCount = Math.Round(predTrips, 2),
                    PredictedStockoutProbability = Math.Round(calculatedTrips > 50 ? 0.78 : 0.14, 4),
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
                .Where(t => t.DeletedAt == null && t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return data.Select(x => (x.ZoneId, x.Count)).ToList();
        }
    }
}
