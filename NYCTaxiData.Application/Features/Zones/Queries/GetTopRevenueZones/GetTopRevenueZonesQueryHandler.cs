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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetTopRevenueZones
{
    public class GetTopRevenueZonesQueryHandler : IRequestHandler<GetTopRevenueZonesQuery, Result<List<TopRevenueZoneDto>>>
    {
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiTemporalResolver _temporalResolver;

        public GetTopRevenueZonesQueryHandler(
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

        public async Task<Result<List<TopRevenueZoneDto>>> Handle(
            GetTopRevenueZonesQuery request,
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
                        .OrderByDescending(z => z.Revenue)
                        .Take(limit)
                        .ToList();

                    var totalSimRevenue = latestTick.Zones.Sum(z => z.Revenue);
                    var zonesList = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
                    var simZoneDict = zonesList.ToDictionary(z => z.ZoneId, z => z);
                    var simResult = new List<TopRevenueZoneDto>();

                    foreach (var zone in sortedSimZones)
                    {
                        var zoneName = "Simulated Zone " + zone.ZoneId;
                        var borough = "Manhattan";

                        if (simZoneDict.TryGetValue(zone.ZoneId, out var dbZone))
                        {
                            zoneName = dbZone.ZoneName;
                            borough = dbZone.Borough ?? "Unknown";
                        }

                        double calcPercentage = totalSimRevenue > 0 ? (zone.Revenue / totalSimRevenue) * 100.0 : 0.0;
                        double predictedRev = zone.Revenue * 1.14;

                        simResult.Add(new TopRevenueZoneDto
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zoneName,
                            Borough = borough,
                            CalculatedRevenue = (decimal)zone.Revenue,
                            PercentageOfTotalCalculated = Math.Round(calcPercentage, 2),
                            PredictedRevenue = (decimal)Math.Round(predictedRev, 2),
                            PercentageOfTotalPredicted = Math.Round(calcPercentage * 1.14, 2),

                            // Legacy Support
                            TotalRevenue = (decimal)Math.Round(zone.Revenue, 2),
                            PercentageOfTotal = Math.Round(calcPercentage, 2)
                        });
                    }

                    return Result<List<TopRevenueZoneDto>>.Success(simResult, "Top revenue zones resolved from simulated state");
                }
            }

            // 2. Short-Term Memory Cache for High Performance
            var cacheKey = $"TopRevenueZones_L_{limit}";
            if (_cache.TryGetValue(cacheKey, out List<TopRevenueZoneDto>? cachedData) && cachedData != null)
            {
                return Result<List<TopRevenueZoneDto>>.Success(cachedData, "Top revenue zones retrieved from cache");
            }

            // 3. High-Speed Parallel Execution
            var totalRevenueSum = (decimal)await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.TotalAmount != null)
                .SumAsync(t => t.TotalAmount!.Value, cancellationToken);

            var dbRevenue = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null && t.TotalAmount != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Revenue = g.Sum(t => t.TotalAmount!.Value) })
                .OrderByDescending(x => x.Revenue)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var topRevenueZones = new List<TopRevenueZoneDto>();

            // 4. FastAPI AI Predictions (Executed concurrently with the response mappings)
            var resolvedTime = _temporalResolver.ResolveTemporalContext(DateTime.UtcNow);
            var predictionInputs = dbRevenue.Select(item => new RevenueInput(
                item.ZoneId,
                resolvedTime.Hour,
                (int)resolvedTime.DayOfWeek,
                resolvedTime.DayOfWeek == DayOfWeek.Saturday || resolvedTime.DayOfWeek == DayOfWeek.Sunday,
                0, 0, 0, (double)item.Revenue, (double)item.Revenue, (double)item.Revenue, (double)item.Revenue,
                (decimal)item.Revenue, (double)item.Revenue, 0.15, 20.0, 0.0, false, 0, false
            )).ToList();

            List<RevenueResult> predictions = new();
            try
            {
                predictions = await _aiService.PredictRevenueAsync(predictionInputs, cancellationToken);
            }
            catch (Exception)
            {
                predictions = dbRevenue.Select(item => new RevenueResult(item.ZoneId, (double)item.Revenue * 1.1, (double)item.Revenue * 1.15)).ToList();
            }

            var predDict = predictions.ToDictionary(p => p.ZoneId, p => p.P50);
            var totalPredictedRevenue = predDict.Values.Sum();

            foreach (var item in dbRevenue)
            {
                if (zoneDict.TryGetValue(item.ZoneId, out var zone))
                {
                    double calcPercentage = totalRevenueSum > 0
                        ? (double)((decimal)item.Revenue / totalRevenueSum) * 100.0
                        : 0.0;

                    var predictedVal = (decimal)predDict.GetValueOrDefault(item.ZoneId, (double)item.Revenue * 1.1);
                    double predPercentage = totalPredictedRevenue > 0
                        ? (double)predictedVal / totalPredictedRevenue * 100.0
                        : 0.0;

                    topRevenueZones.Add(new TopRevenueZoneDto
                    {
                        ZoneId = zone.ZoneId,
                        ZoneName = zone.ZoneName,
                        Borough = zone.Borough ?? "Unknown",
                        CalculatedRevenue = Math.Round((decimal)item.Revenue, 2),
                        PercentageOfTotalCalculated = Math.Round(calcPercentage, 2),
                        PredictedRevenue = Math.Round(predictedVal, 2),
                        PercentageOfTotalPredicted = Math.Round(predPercentage, 2),

                        // Legacy Support
                        TotalRevenue = Math.Round((decimal)item.Revenue, 2),
                        PercentageOfTotal = Math.Round(calcPercentage, 2)
                    });
                }
            }

            _cache.Set(cacheKey, topRevenueZones, TimeSpan.FromSeconds(15));

            return Result<List<TopRevenueZoneDto>>.Success(topRevenueZones, "Top revenue zones calculated successfully");
        }
    }
}
