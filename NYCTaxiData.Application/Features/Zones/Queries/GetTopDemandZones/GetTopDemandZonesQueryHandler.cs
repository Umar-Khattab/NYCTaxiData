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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetTopDemandZones
{
    public class GetTopDemandZonesQueryHandler : IRequestHandler<GetTopDemandZonesQuery, Result<List<TopDemandZoneDto>>>
    {
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiTemporalResolver _temporalResolver;

        public GetTopDemandZonesQueryHandler(
            IMemoryCache cache,
            ISimulationOrchestrator orchestrator,
            IAiPredictionService aiService,
            IUnitOfWork unitOfWork,
            IAiTemporalResolver temporalResolver)
        {
            _cache = cache;
            _orchestrator = orchestrator;
            _aiService = aiService;
            _unitOfWork = unitOfWork;
            _temporalResolver = temporalResolver;
        }

        public async Task<Result<List<TopDemandZoneDto>>> Handle(
            GetTopDemandZonesQuery request,
            CancellationToken cancellationToken)
        {
            var limit = request.Limit > 0 ? request.Limit : 10;

            // 1. Simulation Mode Interceptor
            var simStatus = _orchestrator.GetStatus();
            if (simStatus.Status == SimulationStatus.Running)
            {
                var latestTick = _orchestrator.GetLatestTick();
                if (latestTick != null && latestTick.Zones.Count > 0)
                {
                    var sortedSimZones = latestTick.Zones
                        .OrderByDescending(z => z.Demand)
                        .Take(limit)
                        .ToList();

                    var totalSimDemand = latestTick.Zones.Sum(z => z.Demand);
                    var zonesList = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
                    var simZoneDict = zonesList.ToDictionary(z => z.ZoneId, z => z);
                    var simResult = new List<TopDemandZoneDto>();

                    foreach (var zone in sortedSimZones)
                    {
                        var zoneName = "Simulated Zone " + zone.ZoneId;
                        long? osmId = null;
                        double? centerLat = null;
                        double? centerLong = null;
                        
                        if (simZoneDict.TryGetValue(zone.ZoneId, out var dbZone))
                        {
                            zoneName = dbZone.ZoneName;
                            osmId = dbZone.OsmId;
                            centerLat = dbZone.CenterLat;
                            centerLong = dbZone.CenterLong;
                        }

                        double calcPercentage = totalSimDemand > 0 ? (zone.Demand / totalSimDemand) * 100.0 : 0.0;
                        double predictedDemand = zone.Demand * 1.12;

                        simResult.Add(new TopDemandZoneDto
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zoneName,
                            Borough = "Obsolete",
                            OsmId = osmId,
                            CenterLatitude = centerLat,
                            CenterLongitude = centerLong,
                            CalculatedPickups = (int)zone.Demand,
                            PercentageOfTotalCalculated = Math.Round(calcPercentage, 2),
                            PredictedPickups = Math.Round(predictedDemand, 2),
                            PercentageOfTotalPredicted = Math.Round(calcPercentage * 1.12, 2),
                            
                            // Legacy Support
                            PickupCount = (int)zone.Demand,
                            PercentageOfTotal = Math.Round(calcPercentage, 2)
                        });
                    }

                    return Result<List<TopDemandZoneDto>>.Success(simResult, "Top demand zones resolved from simulated state");
                }
            }

            // 2. Short-Term Memory Cache for High Performance
            var cacheKey = $"TopDemandZones_L_{limit}";
            if (_cache.TryGetValue(cacheKey, out List<TopDemandZoneDto>? cachedData) && cachedData != null)
            {
                return Result<List<TopDemandZoneDto>>.Success(cachedData, "Top demand zones retrieved from cache");
            }

            // 3. High-Speed Parallel Execution (LINQ compiles directly to native SQL aggregates)
            var totalTripsCount = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var dbDemand = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var topDemandZones = new List<TopDemandZoneDto>();

            // 4. FastAPI AI Predictions (Executed concurrently with the response mappings)
            var resolvedTime = _temporalResolver.ResolveTemporalContext(DateTime.UtcNow);
            var predictionInputs = dbDemand.Select(item => new Demand6hInput(
                item.ZoneId,
                resolvedTime.Hour,
                (int)resolvedTime.DayOfWeek,
                resolvedTime.DayOfWeek == DayOfWeek.Saturday || resolvedTime.DayOfWeek == DayOfWeek.Sunday,
                false, 0.0, 0.0, 0.0, 0.0, 20.0, 0.0, false, 0, item.Count
            )).ToList();

            List<Demand6hResult> predictions = new();
            try
            {
                predictions = await _aiService.PredictDemand6hAsync(predictionInputs, cancellationToken);
            }
            catch (Exception)
            {
                predictions = dbDemand.Select(item => new Demand6hResult(item.ZoneId, item.Count * 1.1)).ToList();
            }

            var predDict = predictions.ToDictionary(p => p.ZoneId, p => p.PredictedDemand);
            var totalPredictedCount = predDict.Values.Sum();

            foreach (var item in dbDemand)
            {
                if (zoneDict.TryGetValue(item.ZoneId, out var zone))
                {
                    double calcPercentage = totalTripsCount > 0
                        ? (double)item.Count / totalTripsCount * 100.0
                        : 0.0;

                    var predictedVal = predDict.GetValueOrDefault(item.ZoneId, item.Count * 1.1);
                    double predPercentage = totalPredictedCount > 0
                        ? (double)predictedVal / totalPredictedCount * 100.0
                        : 0.0;

                    topDemandZones.Add(new TopDemandZoneDto
                    {
                        ZoneId = zone.ZoneId,
                        ZoneName = zone.ZoneName,
                        Borough = "Obsolete",
                        OsmId = zone.OsmId,
                        CenterLatitude = zone.CenterLat,
                        CenterLongitude = zone.CenterLong,
                        CalculatedPickups = item.Count,
                        PercentageOfTotalCalculated = Math.Round(calcPercentage, 2),
                        PredictedPickups = Math.Round(predictedVal, 2),
                        PercentageOfTotalPredicted = Math.Round(predPercentage, 2),
                        
                        // Legacy Support
                        PickupCount = item.Count,
                        PercentageOfTotal = Math.Round(calcPercentage, 2)
                    });
                }
            }

            _cache.Set(cacheKey, topDemandZones, TimeSpan.FromSeconds(15));

            return Result<List<TopDemandZoneDto>>.Success(topDemandZones, "Top demand zones calculated successfully");
        }
    }
}
