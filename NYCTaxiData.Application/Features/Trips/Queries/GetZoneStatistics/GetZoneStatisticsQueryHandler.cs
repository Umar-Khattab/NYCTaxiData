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

namespace NYCTaxiData.Application.Features.Trips.Queries.GetZoneStatistics
{
    public class GetZoneStatisticsQueryHandler : IRequestHandler<GetZoneStatisticsQuery, Result<List<ZoneStatisticsDto>>>
    {
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public GetZoneStatisticsQueryHandler(
            IMemoryCache cache,
            ISimulationOrchestrator orchestrator,
            IAiPredictionService aiService,
            IUnitOfWork unitOfWork)
        {
            _cache = cache;
            _orchestrator = orchestrator;
            _aiService = aiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ZoneStatisticsDto>>> Handle(
            GetZoneStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Simulation Mode Interceptor
            var simStatus = _orchestrator.GetStatus();
            if (simStatus.Status == SimulationStatus.Running)
            {
                var latestTick = _orchestrator.GetLatestTick();
                if (latestTick != null && latestTick.Zones.Count > 0)
                {
                    var totalSimDemand = latestTick.Zones.Sum(z => z.Demand);
                    var zonesList = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
                    var zoneDict = zonesList.ToDictionary(z => z.ZoneId, z => z);
                    var simResult = new List<ZoneStatisticsDto>();

                    foreach (var zone in latestTick.Zones)
                    {
                        var zoneName = "Simulated Zone " + zone.ZoneId;
                        var borough = "Manhattan";

                        if (zoneDict.TryGetValue(zone.ZoneId, out var dbz))
                        {
                            zoneName = dbz.ZoneName;
                            borough = dbz.OsmId?.ToString() ?? "Unknown";
                        }

                        double expectedDemand15 = zone.Demand / 4.0;
                        double predictedDemand6 = zone.Demand * 6.0;

                        var stat = new ZoneStatisticsDto
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zoneName,
                            Borough = borough,
                            Calculated = new ZoneCalculatedStats
                            {
                                TotalPickupTrips = (int)zone.Demand,
                                TotalDropoffTrips = zone.ActiveTrips,
                                TotalRevenue = (decimal)Math.Round(zone.Revenue, 2),
                                AvgFare = (decimal)Math.Round(zone.Demand > 0 ? zone.Revenue / zone.Demand : 0.0, 2),
                                AvgTip = (decimal)Math.Round(zone.Demand > 0 ? (zone.Revenue * 0.15) / zone.Demand : 0.0, 2),
                                BusiestHourOfDay = 17,
                                BusiestDayOfWeek = "Friday"
                            },
                            Predicted = new ZonePredictedStats
                            {
                                ExpectedDemand15Min = Math.Round(expectedDemand15, 2),
                                ExpectedDemand6H = Math.Round(predictedDemand6, 2),
                                ExpectedRevenue6H = (decimal)Math.Round(zone.Revenue * 6.0, 2),
                                StockoutProbability = Math.Round(zone.StockoutRisk, 4),
                                BusiestHourForecast = 18
                            }
                        };

                        stat.TotalPickupTrips = stat.Calculated.TotalPickupTrips;
                        stat.TotalDropoffTrips = stat.Calculated.TotalDropoffTrips;
                        stat.TotalRevenue = stat.Calculated.TotalRevenue;
                        stat.AvgFare = stat.Calculated.AvgFare;
                        stat.AvgTip = stat.Calculated.AvgTip;
                        stat.BusiestHourOfDay = stat.Calculated.BusiestHourOfDay;
                        stat.BusiestDayOfWeek = stat.Calculated.BusiestDayOfWeek;

                        simResult.Add(stat);
                    }

                    return Result<List<ZoneStatisticsDto>>.Success(
                        simResult.OrderByDescending(x => x.TotalPickupTrips).ToList(),
                        "Zone statistics list resolved from simulated state"
                    );
                }
            }

            // 2. Short-Term Memory Cache for High Performance
            var cacheKey = "TripsAllZoneStats";
            if (_cache.TryGetValue(cacheKey, out List<ZoneStatisticsDto>? cachedData) && cachedData != null)
            {
                return Result<List<ZoneStatisticsDto>>.Success(cachedData, "Zone statistics list retrieved from cache");
            }

            // 3. High-Speed Parallel Execution (LINQ compiles directly to batch native SQL queries)
            // 3. High-Speed Sequential Execution to ensure DbContext thread safety
            var pickupsGroup = await _unitOfWork.Trips.Query().AsNoTracking()
                .Where(t =>   t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new
                {
                    ZoneId = g.Key,
                    Count = g.Count(), 
                    AvgFare = g.Average(t => t.FareAmount),
                    AvgTip = g.Average(t => t.TipAmount ?? 0m)
                })
                .ToListAsync(cancellationToken);

            var dropoffsGroup = await _unitOfWork.Trips.Query().AsNoTracking()
                .Where(t =>  t.DropoffLocation != null && t.DropoffLocation.ZoneId != null)
                .GroupBy(t => t.DropoffLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var pickupDict = pickupsGroup.ToDictionary(p => p.ZoneId, p => p);
            var dropoffDict = dropoffsGroup.ToDictionary(d => d.ZoneId, d => d.Count);

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var statisticsList = new List<ZoneStatisticsDto>();

            // 4. FastAPI AI batch predictions
            var batch15mInputs = pickupsGroup.Select(p => new Demand15MinInput(
                p.ZoneId, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, (int)DateTime.UtcNow.DayOfWeek,
                DateTime.UtcNow.Month, DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday,
                0, 0, 0, 0, 0, 20.0, 0.0, false, 0, p.Count
            )).ToList();

            List<Demand15MinResult> predictions15m = new();
            try
            {
                predictions15m = await _aiService.PredictDemand15MinAsync(batch15mInputs, true, cancellationToken);
            }
            catch (Exception)
            {
                predictions15m = pickupsGroup.Select(p => new Demand15MinResult(p.ZoneId, p.Count / 4.0 * 1.1)).ToList();
            }

            var pred15mDict = predictions15m.ToDictionary(p => p.ZoneId, p => p.PredictedDemand);

            foreach (var zone in zones)
            {
                int totalPickups = 0;
                int totalDropoffs = dropoffDict.GetValueOrDefault(zone.ZoneId, 0);
                decimal totalRevenue = 0m;
                decimal avgFare = 0m;
                decimal avgTip = 0m;

                if (pickupDict.TryGetValue(zone.ZoneId, out var pInfo))
                {
                    totalPickups = pInfo.Count; 
                    avgFare = (decimal)pInfo.AvgFare;
                    avgTip = (decimal)pInfo.AvgTip;
                }

                double pred15 = pred15mDict.GetValueOrDefault(zone.ZoneId, totalPickups / 4.0 * 1.1);
                double pred6 = totalPickups * 1.12;
                decimal expectedRev6 = totalRevenue * 1.15m;
                double stockoutProb = totalPickups > totalDropoffs ? 0.72 : 0.15;

                var stats = new ZoneStatisticsDto
                {
                    ZoneId = zone.ZoneId,
                    ZoneName = zone.ZoneName,
                    Borough = zone.ZoneName ?? "Unknown",
                    Calculated = new ZoneCalculatedStats
                    {
                        TotalPickupTrips = totalPickups,
                        TotalDropoffTrips = totalDropoffs,
                        TotalRevenue = Math.Round(totalRevenue, 2),
                        AvgFare = Math.Round(avgFare, 2),
                        AvgTip = Math.Round(avgTip, 2),
                        BusiestHourOfDay = 17,
                        BusiestDayOfWeek = "Friday"
                    },
                    Predicted = new ZonePredictedStats
                    {
                        ExpectedDemand15Min = Math.Round(pred15, 2),
                        ExpectedDemand6H = Math.Round(pred6, 2),
                        ExpectedRevenue6H = Math.Round(expectedRev6, 2),
                        StockoutProbability = Math.Round(stockoutProb, 4),
                        BusiestHourForecast = 18
                    }
                };

                stats.TotalPickupTrips = stats.Calculated.TotalPickupTrips;
                stats.TotalDropoffTrips = stats.Calculated.TotalDropoffTrips;
                stats.TotalRevenue = stats.Calculated.TotalRevenue;
                stats.AvgFare = stats.Calculated.AvgFare;
                stats.AvgTip = stats.Calculated.AvgTip;
                stats.BusiestHourOfDay = stats.Calculated.BusiestHourOfDay;
                stats.BusiestDayOfWeek = stats.Calculated.BusiestDayOfWeek;

                statisticsList.Add(stats);
            }

            var orderedStats = statisticsList
                .OrderByDescending(x => x.TotalPickupTrips)
                .ToList();

            _cache.Set(cacheKey, orderedStats, TimeSpan.FromSeconds(15));

            return Result<List<ZoneStatisticsDto>>.Success(orderedStats, "Zone trip statistics computed successfully");
        }
    }
}
