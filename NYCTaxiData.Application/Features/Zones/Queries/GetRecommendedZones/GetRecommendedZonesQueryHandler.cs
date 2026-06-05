using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetRecommendedZones
{
    public class GetRecommendedZonesQueryHandler : IRequestHandler<GetRecommendedZonesQuery, Result<List<RecommendedZoneDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;
        private readonly IAiFeatureProvider _aiFeatureProvider;
        private readonly ILogger<GetRecommendedZonesQueryHandler> _logger;

        public GetRecommendedZonesQueryHandler(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ISimulationOrchestrator orchestrator,
            IAiPredictionService aiService,
            IAiFeatureProvider aiFeatureProvider,
            ILogger<GetRecommendedZonesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _orchestrator = orchestrator;
            _aiService = aiService;
            _aiFeatureProvider = aiFeatureProvider;
            _logger = logger;
        }

        public async Task<Result<List<RecommendedZoneDto>>> Handle(
            GetRecommendedZonesQuery request,
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
                    // In simulation, compute simulated opportunity score: Demand - DriverCount
                    var sortedSimZones = latestTick.Zones
                        .OrderByDescending(z => z.Demand - z.DriverCount)
                        .Take(limit)
                        .ToList();

                    var simResult = new List<RecommendedZoneDto>();

                    // Fix N+1 query: fetch all required zones at once
                    var simZoneIds = sortedSimZones.Select(z => z.ZoneId).ToList();
                    var dbZones = await _unitOfWork.Zones.Query()
                        .AsNoTracking()
                        .Where(z => simZoneIds.Contains(z.ZoneId))
                        .ToListAsync(cancellationToken);
                    var dbZoneDict = dbZones.ToDictionary(z => z.ZoneId, z => z);

                    foreach (var zone in sortedSimZones)
                    {
                        var zoneName = "Simulated Zone " + zone.ZoneId;
                        long? osmId = null;
                        double? centerLat = null;
                        double? centerLng = null;

                        if (dbZoneDict.TryGetValue(zone.ZoneId, out var dbZone))
                        {
                            zoneName = dbZone.ZoneName ?? zoneName;
                            osmId = dbZone.OsmId;
                            centerLat = dbZone.CenterLat;
                            centerLng = dbZone.CenterLong;
                        }

                        double ratio = zone.DriverCount > 0 ? zone.Demand / zone.DriverCount : zone.Demand;
                        double score = Math.Clamp((ratio / 2.0) * 100.0, 10.0, 99.0);

                        simResult.Add(new RecommendedZoneDto
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zoneName,
                            OsmId = osmId,
                            CenterLatitude = centerLat,
                            CenterLongitude = centerLng,
                            RecommendationScore = Math.Round((decimal)score, 2),
                            DemandSupplyRatio = Math.Round((decimal)ratio, 2),
                            PredictedRevenueYield = (decimal)Math.Round(zone.Revenue * 1.15, 2),
                            Reason = "High simulated demand opportunity with driver shortage.",
                            
                            // Legacy Support
                            AvgFare = (decimal)Math.Round(zone.Demand > 0 ? zone.Revenue / zone.Demand : 14.50, 2),
                            AvgTip = (decimal)Math.Round(zone.Demand > 0 ? (zone.Revenue * 0.15) / zone.Demand : 2.50, 2)
                        });
                    }

                    return Result<List<RecommendedZoneDto>>.Success(simResult, "Recommended zones resolved from simulated state");
                }
            }

            // 2. Short-Term Memory Cache for High Performance
            var cacheKey = $"RecommendedZones_L_{limit}";
            if (_cache.TryGetValue(cacheKey, out List<RecommendedZoneDto>? cachedData) && cachedData != null)
            {
                return Result<List<RecommendedZoneDto>>.Success(cachedData, "Recommended zones retrieved from cache");
            }

            // 3. Fetch Operational Active States via Consolidated Database Queries
            var recentTime = DateTime.UtcNow.AddHours(-24);
            var consolidatedData = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => (t.StartedAt >= recentTime || t.ProcessStatus == "Ongoing")
                         && t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new
                {
                    ZoneId = g.Key,
                    PickupCount = g.Count(t => t.StartedAt >= recentTime),
                    Revenue = g.Where(t => t.StartedAt >= recentTime).Sum(t => t.FareAmount ?? 0),
                    ActiveCount = g.Count(t => t.ProcessStatus == "Ongoing")
                })
                .ToListAsync(cancellationToken);

            var demandDict = consolidatedData.ToDictionary(x => x.ZoneId, x => x.PickupCount);
            var revenueDict = consolidatedData.ToDictionary(x => x.ZoneId, x => x.Revenue);
            var activeTripsDict = consolidatedData.ToDictionary(x => x.ZoneId, x => x.ActiveCount);

            var supplyList = await GetDriverSupplyPerZoneNativelyAsync(cancellationToken);
            var driverSupplyDict = supplyList.ToDictionary(s => s.ZoneId, s => s.DriverCount);

            const string zonesCacheKey = "ZoneDistribution_ZonesList";
            if (!_cache.TryGetValue(zonesCacheKey, out List<Zone>? allZones) || allZones == null)
            {
                allZones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
                _cache.Set(zonesCacheKey, allZones, TimeSpan.FromHours(1));
            }
            var zones = allZones.Where(z => z.ZoneId >= 1 && z.ZoneId <= 265).ToList();
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var recommendations = new List<RecommendedZoneDto>();

            // 4. FastAPI AI profit maximization decision engine (The Sole Source of Truth)
            var targetTime = DateTime.UtcNow;
            var zoneIds = zones.Select(z => z.ZoneId).ToList();

            var features6h = await _aiFeatureProvider.GetDemand6hFeaturesAsync(zoneIds, targetTime, cancellationToken);
            var featuresRev = await _aiFeatureProvider.GetRevenueFeaturesAsync(zoneIds, targetTime, cancellationToken);
            var featuresStock = await _aiFeatureProvider.GetStockOutFeaturesAsync(zoneIds, targetTime, cancellationToken);

            var dict6h = features6h.ToDictionary(f => f.ZoneId, f => f);
            var dictRev = featuresRev.ToDictionary(f => f.ZoneId, f => f);
            var dictStock = featuresStock.ToDictionary(f => f.ZoneId, f => f);

            var pmInputs = new List<ProfitMaximizationInput>();
            var targetDateTimeStr = targetTime.ToString("yyyy-MM-dd HH:mm:ss");

            foreach (var z in zones)
            {
                var f6h = dict6h.GetValueOrDefault(z.ZoneId);
                var fRev = dictRev.GetValueOrDefault(z.ZoneId);
                var fStock = dictStock.GetValueOrDefault(z.ZoneId);

                int currentDrivers = driverSupplyDict.GetValueOrDefault(z.ZoneId, 0);
                bool allowAsSource = true;
                bool allowAsTarget = true;
                bool isEventZone = false;
                bool isAirportZone = z.ZoneName.Contains("Airport", StringComparison.OrdinalIgnoreCase) || z.ZoneId == 132 || z.ZoneId == 138 || z.ZoneId == 1;

                pmInputs.Add(new ProfitMaximizationInput(
                    z.ZoneId,
                    currentDrivers,
                    allowAsSource,
                    allowAsTarget,
                    isEventZone,
                    isAirportZone,
                    targetTime.Hour,
                    (int)targetTime.DayOfWeek,
                    targetTime.DayOfWeek == DayOfWeek.Saturday || targetTime.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                    f6h?.TempC ?? 20.0,
                    f6h?.RainMm ?? 0.0,
                    (f6h?.IsRain ?? false) ? 1 : 0,
                    f6h?.WeatherCode ?? 0,
                    (f6h?.IsHoliday ?? false) ? 1 : 0,
                    f6h?.Lag1_6h ?? 0.0,
                    f6h?.Lag2_6h ?? 0.0,
                    f6h?.Lag4_6h ?? 0.0,
                    f6h?.RollingMean24h ?? 0.0,
                    fRev?.RevLag1_6h ?? 0.0,
                    fRev?.RevLag1Week ?? 0.0,
                    fRev?.RevRollingMean7d ?? 0.0,
                    fRev?.RevRollingMean30d ?? 0.0,
                    fRev?.AvgFare ?? 0.0,
                    fRev?.TipRate ?? 0.15,
                    f6h?.PickupCount ?? 0,
                    (int)(fStock?.DropoffCount ?? 0),
                    fStock?.NetFlow ?? 0.0,
                    fStock?.ActivityRatio ?? 1.0,
                    fStock?.Lag1Pickup ?? 0.0,
                    fStock?.Lag1Dropoff ?? 0.0,
                    fStock?.Lag1NetFlow ?? 0.0
                ));
            }

            ProfitMaximizationResult? profitResult = null;
            try
            {
                int currentZoneId = zoneIds.FirstOrDefault(id => id >= 1 && id <= 265);
                if (currentZoneId == 0) currentZoneId = 1;
                profitResult = await _aiService.MaximizeProfitAsync(targetDateTimeStr, currentZoneId, pmInputs, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to call external FastAPI model endpoints for GetRecommendedZones. Using local fallback.");
            }

            if (profitResult != null && profitResult.ZoneEvaluations.Count > 0)
            {
                _cache.Set("Shared_ProfitPlanEvaluations", profitResult.ZoneEvaluations, TimeSpan.FromSeconds(60));

                foreach (var eval in profitResult.ZoneEvaluations)
                {
                    if (zoneDict.TryGetValue(eval.ZoneId, out var zone))
                    {
                        double gapScore = eval.DriverGap > 0 ? (eval.DriverGap / (double)Math.Max(1, eval.DriversNeeded6h)) * 50.0 : 0.0;
                        double stockoutScore = eval.StockoutProb * 30.0;
                        double servedScore = (1.0 - eval.ServedRatioBaseline) * 20.0;
                        double score = Math.Clamp(gapScore + stockoutScore + servedScore + 30.0, 10.0, 99.0);

                        double ratio = eval.CurrentDrivers > 0 ? eval.Demand6h / eval.CurrentDrivers : eval.Demand6h;

                        string reason = !string.IsNullOrEmpty(eval.Reason) 
                            ? eval.Reason 
                            : (eval.TargetCandidate ? "High profit opportunity with driver gap." : "Stable driver demand/supply balance.");

                        recommendations.Add(new RecommendedZoneDto
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zone.ZoneName,
                            OsmId = zone.OsmId,
                            CenterLatitude = zone.CenterLat,
                            CenterLongitude = zone.CenterLong,
                            RecommendationScore = Math.Round((decimal)score, 2),
                            DemandSupplyRatio = Math.Round((decimal)ratio, 2),
                            PredictedRevenueYield = Math.Round((decimal)eval.RevenueP50, 2),
                            Reason = reason,
                            
                            // Legacy Support
                            AvgFare = Math.Round((decimal)eval.RevenueP50, 2),
                            AvgTip = Math.Round((decimal)(eval.RevenueP50 * 0.15), 2)
                        });
                    }
                }
            }
            else
            {
                // Resilient C# Normalized Fallback Score Formula (Local computations)
                double maxTrips = demandDict.Values.DefaultIfEmpty(1).Max();
                decimal maxFare = revenueDict.Values.DefaultIfEmpty(1m).Max();

                foreach (var z in zones)
                {
                    int pickups = demandDict.GetValueOrDefault(z.ZoneId, 0);
                    decimal revenue = revenueDict.GetValueOrDefault(z.ZoneId, 0m);
                    int activeDrivers = driverSupplyDict.GetValueOrDefault(z.ZoneId, 0);

                    double normTrips = pickups / maxTrips;
                    double normFare = maxFare > 0 ? (double)(revenue / maxFare) : 0.0;

                    double score = (normFare * 0.55) + (normTrips * 0.45);
                    decimal finalScore = Math.Round((decimal)(score * 100.0), 2);
                    
                    double ratio = activeDrivers > 0 ? (double)pickups / activeDrivers : pickups;

                    recommendations.Add(new RecommendedZoneDto
                    {
                        ZoneId = z.ZoneId,
                        ZoneName = z.ZoneName,
                        OsmId = z.OsmId,
                        CenterLatitude = z.CenterLat,
                        CenterLongitude = z.CenterLong,
                        RecommendationScore = finalScore,
                        DemandSupplyRatio = Math.Round((decimal)ratio, 2),
                        PredictedRevenueYield = Math.Round(revenue * 1.12m, 2),
                        Reason = normFare > 0.7 ? "Premium fare profiles historically detected." : "Strong typical passenger demand volumes.",
                        
                        // Legacy Support
                        AvgFare = Math.Round(revenue / Math.Max(1, pickups), 2),
                        AvgTip = Math.Round((revenue * 0.15m) / Math.Max(1, pickups), 2)
                    });
                }
            }

            var sortedRecommendations = recommendations
                .OrderByDescending(x => x.RecommendationScore)
                .Take(limit)
                .ToList();

            // Cache result for 15 seconds
            _cache.Set(cacheKey, sortedRecommendations, TimeSpan.FromSeconds(15));

            return Result<List<RecommendedZoneDto>>.Success(sortedRecommendations, "Recommended zones generated successfully");
        }


        private async Task<List<(int ZoneId, int DriverCount)>> GetDriverSupplyPerZoneNativelyAsync(CancellationToken ct)
        {
            var recentTime = DateTime.UtcNow.AddHours(-24);
            var availableStatusStr = CurrentStatus.Available.ToString();

            var availableDrivers = await _unitOfWork.Drivers.Query()
                .AsNoTracking()
                .Where(d => d.Status == availableStatusStr)
                .Select(d => d.UserId)
                .ToListAsync(ct);

            var trips = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.StartedAt >= recentTime && t.DriverId != null && availableDrivers.Contains(t.DriverId.Value) && t.DropoffLocation != null && t.DropoffLocation.ZoneId != null)
                .Select(t => new { t.DriverId, t.StartedAt, ZoneId = t.DropoffLocation!.ZoneId!.Value })
                .ToListAsync(ct);

            var driverSupply = trips
                .GroupBy(t => t.DriverId)
                .Select(g => g.OrderByDescending(t => t.StartedAt).First().ZoneId)
                .GroupBy(zoneId => zoneId)
                .Select(g => (ZoneId: g.Key, DriverCount: g.Count()))
                .ToList();

            return driverSupply;
        }


    }
}
