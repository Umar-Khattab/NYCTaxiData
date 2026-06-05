using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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

namespace NYCTaxiData.Application.Features.Zones.Queries.CompareZones
{
    public class CompareZonesQueryHandler : IRequestHandler<CompareZonesQuery, Result<ZoneComparisonDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;
        private readonly IAiFeatureProvider _aiFeatureProvider;
        private readonly ILogger<CompareZonesQueryHandler> _logger;

        public CompareZonesQueryHandler(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ISimulationOrchestrator orchestrator,
            IAiPredictionService aiService,
            IAiFeatureProvider aiFeatureProvider,
            ILogger<CompareZonesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _orchestrator = orchestrator;
            _aiService = aiService;
            _aiFeatureProvider = aiFeatureProvider;
            _logger = logger;
        }

        public async Task<Result<ZoneComparisonDto>> Handle(
            CompareZonesQuery request,
            CancellationToken cancellationToken)
        {
            if (request.ZoneIds == null || request.ZoneIds.Count == 0)
                return Result<ZoneComparisonDto>.Failure("Zone IDs must be provided for comparison", "Validation");

            var uniqueZoneIds = request.ZoneIds.Distinct().ToList();

            // 1. Simulation Mode Interceptor
            var simStatus = _orchestrator.GetStatus();
            if (simStatus.Status == SimulationStatus.Running)
            {
                var simStatsList = new List<ZoneStatisticsDto>();
                var zonesList = await _unitOfWork.Zones.Query().AsNoTracking().Where(z => uniqueZoneIds.Contains(z.ZoneId)).ToListAsync(cancellationToken);
                var zoneDictSim = zonesList.ToDictionary(z => z.ZoneId, z => z);

                foreach (var zId in uniqueZoneIds)
                {
                    var zoneHistory = _orchestrator.GetZoneHistory(zId);
                    zoneDictSim.TryGetValue(zId, out var dbZone);
                    var zoneName = dbZone != null ? dbZone.ZoneName : "Simulated Zone " + zId;
                    int tripCount = 0;
                    double revenue = 0.0;
                    double avgFare = 0.0;
                    double stockoutRisk = 0.15;

                    if (zoneHistory != null && zoneHistory.Points.Count > 0)
                    {
                        var lastPoint = zoneHistory.Points[^1];
                        tripCount = (int)lastPoint.Demand;
                        revenue = lastPoint.Revenue;
                        avgFare = tripCount > 0 ? revenue / tripCount : 0.0;
                        stockoutRisk = lastPoint.StockoutRisk;
                    }

                    var stat = new ZoneStatisticsDto
                    {
                        ZoneId = zId,
                        ZoneName = zoneName,
                        Calculated = new ZoneCalculatedStats
                        {
                            TotalPickupTrips = tripCount,
                            TotalDropoffTrips = tripCount,
                            TotalRevenue = (decimal)Math.Round(revenue, 2),
                            AvgFare = (decimal)Math.Round(avgFare, 2),
                            AvgTip = (decimal)Math.Round(avgFare * 0.15, 2),
                            BusiestHourOfDay = 17,
                            BusiestDayOfWeek = "Friday"
                        },
                        Predicted = new ZonePredictedStats
                        {
                            ExpectedDemand15Min = Math.Round(tripCount / 4.0, 2),
                            ExpectedDemand6H = Math.Round(tripCount * 6.0, 2),
                            ExpectedRevenue6H = (decimal)Math.Round(revenue * 6.0, 2),
                            StockoutProbability = Math.Round(stockoutRisk, 4),
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

                    simStatsList.Add(stat);
                }

                if (simStatsList.Count == 0)
                    return Result<ZoneComparisonDto>.Failure("None of the specified zones were found", "NotFound");

                var simWinnerByRevenue = simStatsList.OrderByDescending(x => x.TotalRevenue).First();
                var simWinnerByTrips = simStatsList.OrderByDescending(x => x.TotalPickupTrips).First();

                var simComparison = new ZoneComparisonDto
                {
                    ComparisonData = simStatsList,
                    HighestRevenueZone = simWinnerByRevenue.ZoneName,
                    BusiestZone = simWinnerByTrips.ZoneName
                };

                return Result<ZoneComparisonDto>.Success(simComparison, "Zone comparison completed successfully via simulated states");
            }

            // 2. Short-Term Memory Cache for High Performance
            var joinedIds = string.Join("-", uniqueZoneIds.OrderBy(id => id));
            var cacheKey = $"CompareZones_Ids_{joinedIds}";
            if (_cache.TryGetValue(cacheKey, out ZoneComparisonDto? cachedData) && cachedData != null)
            {
                return Result<ZoneComparisonDto>.Success(cachedData, "Zone comparison retrieved from cache");
            }

            // 3. Batch High-Speed Database Queries
            var pickups = await GetBatchPickupsNativelyAsync(uniqueZoneIds, cancellationToken);
            var dropoffs = await GetBatchDropoffsNativelyAsync(uniqueZoneIds, cancellationToken);

            var pickupDict = pickups.ToDictionary(p => p.ZoneId, p => p);
            var dropoffDict = dropoffs.ToDictionary(d => d.ZoneId, d => d.Count);

            var dbZones = await _unitOfWork.Zones.Query().AsNoTracking().Where(z => uniqueZoneIds.Contains(z.ZoneId)).ToListAsync(cancellationToken);
            var zoneDict = dbZones.ToDictionary(z => z.ZoneId, z => z);

            var comparisonStats = new List<ZoneStatisticsDto>();

            // 4. Retrieve Engineered Features and Query AI Service Predictions in Parallel
            var targetTime = DateTime.UtcNow;
            var features15m = await _aiFeatureProvider.GetDemand15MinFeaturesAsync(uniqueZoneIds, targetTime, cancellationToken);
            var features6h = await _aiFeatureProvider.GetDemand6hFeaturesAsync(uniqueZoneIds, targetTime, cancellationToken);
            var featuresRev = await _aiFeatureProvider.GetRevenueFeaturesAsync(uniqueZoneIds, targetTime, cancellationToken);
            var featuresStock = await _aiFeatureProvider.GetStockOutFeaturesAsync(uniqueZoneIds, targetTime, cancellationToken);

            var task15m = _aiService.PredictDemand15MinAsync(features15m, true, cancellationToken);
            var task6h = _aiService.PredictDemand6hAsync(features6h, cancellationToken);
            var taskRev = _aiService.PredictRevenueAsync(featuresRev, cancellationToken);
            var taskStock = _aiService.PredictStockOutAsync(featuresStock, cancellationToken);

            try
            {
                await Task.WhenAll(task15m, task6h, taskRev, taskStock);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to call external FastAPI model endpoints for CompareZones. Using default fallbacks.");
            }

            var pred15mDict = task15m.Status == TaskStatus.RanToCompletion && task15m.Result != null
                ? task15m.Result.ToDictionary(p => p.ZoneId, p => p.PredictedDemand)
                : new Dictionary<int, double>();

            var pred6hDict = task6h.Status == TaskStatus.RanToCompletion && task6h.Result != null
                ? task6h.Result.ToDictionary(p => p.ZoneId, p => p.PredictedDemand)
                : new Dictionary<int, double>();

            var predRevDict = taskRev.Status == TaskStatus.RanToCompletion && taskRev.Result != null
                ? taskRev.Result.ToDictionary(r => r.ZoneId, r => r.P50)
                : new Dictionary<int, double>();

            var stockoutDict = taskStock.Status == TaskStatus.RanToCompletion && taskStock.Result != null
                ? taskStock.Result.ToDictionary(s => s.ZoneId, s => s.Probability)
                : new Dictionary<int, double>();

            foreach (var zoneId in uniqueZoneIds)
            {
                if (!zoneDict.TryGetValue(zoneId, out var zone))
                    continue;

                int totalPickups = 0;
                int totalDropoffs = dropoffDict.GetValueOrDefault(zoneId, 0);
                decimal totalRevenue = 0m;
                decimal avgFare = 0m;
                decimal avgTip = 0m;

                if (pickupDict.TryGetValue(zoneId, out var pInfo))
                {
                    totalPickups = pInfo.Count; 
                    totalRevenue = pInfo.TotalRevenue;
                    avgFare = pInfo.AvgFare; 
                    avgTip = pInfo.AvgTip;
                }

                double pred15m = pred15mDict.GetValueOrDefault(zoneId, totalPickups / 4.0 * 1.15);
                double pred6h = pred6hDict.GetValueOrDefault(zoneId, totalPickups * 1.12);
                decimal predRev6h = predRevDict.ContainsKey(zoneId) 
                    ? (decimal)predRevDict[zoneId] 
                    : totalRevenue * 1.15m;
                double stockoutProb = stockoutDict.GetValueOrDefault(zoneId, totalPickups > totalDropoffs ? 0.65 : 0.12);

                var stats = new ZoneStatisticsDto
                {
                    ZoneId = zoneId,
                    ZoneName = zone.ZoneName,
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
                        ExpectedDemand15Min = Math.Round(pred15m, 2),
                        ExpectedDemand6H = Math.Round(pred6h, 2),
                        ExpectedRevenue6H = Math.Round(predRev6h, 2),
                        StockoutProbability = Math.Round(stockoutProb, 4),
                        BusiestHourForecast = 18
                    }
                };

                // Legacy Mapping (Backwards compatibility)
                stats.TotalPickupTrips = stats.Calculated.TotalPickupTrips;
                stats.TotalDropoffTrips = stats.Calculated.TotalDropoffTrips;
                stats.TotalRevenue = stats.Calculated.TotalRevenue;
                stats.AvgFare = stats.Calculated.AvgFare;
                stats.AvgTip = stats.Calculated.AvgTip;
                stats.BusiestHourOfDay = stats.Calculated.BusiestHourOfDay;
                stats.BusiestDayOfWeek = stats.Calculated.BusiestDayOfWeek;

                comparisonStats.Add(stats);
            }

            if (comparisonStats.Count == 0)
                return Result<ZoneComparisonDto>.Failure("None of the specified zones were found", "NotFound");

            var winnerByRevenue = comparisonStats.OrderByDescending(x => x.TotalRevenue).First();
            var winnerByTrips = comparisonStats.OrderByDescending(x => x.TotalPickupTrips).First();

            var comparisonResult = new ZoneComparisonDto
            {
                ComparisonData = comparisonStats,
                HighestRevenueZone = winnerByRevenue.ZoneName,
                BusiestZone = winnerByTrips.ZoneName
            };

            // Cache result for 15 seconds
            _cache.Set(cacheKey, comparisonResult, TimeSpan.FromSeconds(15));

            return Result<ZoneComparisonDto>.Success(comparisonResult, "Zone comparison completed successfully");
        }

        private async Task<List<BatchPickupResult>> GetBatchPickupsNativelyAsync(List<int> zoneIds, CancellationToken ct)
        {
            return await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null && zoneIds.Contains(t.PickupLocation.ZoneId.Value))
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new BatchPickupResult(
                    g.Key,
                    g.Count(), 
                    g.Sum(t => t.FareAmount ?? 0m),
                    g.Average(t => t.FareAmount ?? 0m),
                    g.Average(t => t.TipAmount ?? 0m) 
                ))
                .ToListAsync(ct);
        }

        private async Task<List<(int ZoneId, int Count)>> GetBatchDropoffsNativelyAsync(List<int> zoneIds, CancellationToken ct)
        {
            var data = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.DropoffLocation != null && t.DropoffLocation.ZoneId != null && zoneIds.Contains(t.DropoffLocation.ZoneId.Value))
                .GroupBy(t => t.DropoffLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return data.Select(x => (x.ZoneId, x.Count)).ToList();
        }

        private record BatchPickupResult(int ZoneId, int Count, decimal TotalRevenue, decimal AvgFare, decimal AvgTip);
    }
}
