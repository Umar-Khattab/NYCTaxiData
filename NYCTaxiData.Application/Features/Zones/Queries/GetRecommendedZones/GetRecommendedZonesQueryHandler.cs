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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetRecommendedZones
{
    public class GetRecommendedZonesQueryHandler : IRequestHandler<GetRecommendedZonesQuery, Result<List<RecommendedZoneDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;

        public GetRecommendedZonesQueryHandler(
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

                    foreach (var zone in sortedSimZones)
                    {
                        var zoneName = "Simulated Zone " + zone.ZoneId;
                        var borough = "Manhattan";

                        var dbZone = await _unitOfWork.Zones.Query().AsNoTracking().FirstOrDefaultAsync(z => z.ZoneId == zone.ZoneId, cancellationToken);
                        if (dbZone != null)
                        {
                            zoneName = dbZone.ZoneName;
                            borough = dbZone.Borough ?? "Unknown";
                        }

                        double ratio = zone.DriverCount > 0 ? zone.Demand / zone.DriverCount : zone.Demand;
                        double score = Math.Clamp((ratio / 2.0) * 100.0, 10.0, 99.0);

                        simResult.Add(new RecommendedZoneDto
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zoneName,
                            Borough = borough,
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

            // 3. Fetch Operational Active States via Concurrent Native Queries
            // 3. Fetch Operational Active States via Sequential Queries to ensure DbContext thread safety
            var demandList = await GetDemandPerZoneNativelyAsync(cancellationToken);
            var supplyList = await GetDriverSupplyPerZoneNativelyAsync(cancellationToken);
            var revenueList = await GetRevenuePerZoneNativelyAsync(cancellationToken);
            var activeTripsList = await GetActiveTripsPerZoneNativelyAsync(cancellationToken);

            var demandDict = demandList.ToDictionary(d => d.ZoneId, d => d.PickupCount);
            var driverSupplyDict = supplyList.ToDictionary(s => s.ZoneId, s => s.DriverCount);
            var revenueDict = revenueList.ToDictionary(r => r.ZoneId, r => r.Revenue);
            var activeTripsDict = activeTripsList.ToDictionary(a => a.ZoneId, a => a.ActiveCount);

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var recommendations = new List<RecommendedZoneDto>();

            // 4. FastAPI Repositioning Plan Optimizer (The Sole Source of Truth)
            var zoneStates = zones.Select(z => new ZoneSupplyState(
                z.ZoneId,
                driverSupplyDict.GetValueOrDefault(z.ZoneId, 0),
                activeTripsDict.GetValueOrDefault(z.ZoneId, 0),
                (double)demandDict.GetValueOrDefault(z.ZoneId, 0),
                0.12, // stockoutRisk resolved by model
                (double)revenueDict.GetValueOrDefault(z.ZoneId, 0m)
            )).ToList();

            RepositioningPlan plan = null;
            try
            {
                plan = await _aiService.OptimizeRepositioningAsync(DateTime.UtcNow, zoneStates, null, cancellationToken);
            }
            catch (Exception)
            {
                // Fallback plan if FastAPI is offline
                plan = null;
            }

            if (plan != null && plan.ZoneSummaries.Count > 0)
            {
                var summaryDict = plan.ZoneSummaries.ToDictionary(s => s.ZoneId, s => s);

                foreach (var zId in summaryDict.Keys)
                {
                    if (zoneDict.TryGetValue(zId, out var zone))
                    {
                        var summary = summaryDict[zId];
                        double ratio = summary.SupplyAfter > 0 ? summary.DemandForecast / summary.SupplyAfter : summary.DemandForecast;
                        double score = Math.Clamp(summary.CoverageRatioAfter * 100.0, 10.0, 99.0);

                        recommendations.Add(new RecommendedZoneDto
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zone.ZoneName,
                            Borough = zone.Borough ?? "Unknown",
                            RecommendationScore = Math.Round((decimal)score, 2),
                            DemandSupplyRatio = Math.Round((decimal)ratio, 2),
                            PredictedRevenueYield = Math.Round(revenueDict.GetValueOrDefault(zone.ZoneId, 0m) * 1.15m, 2),
                            Reason = summary.CoverageRatioAfter > 0.8 ? "High optimized demand coverage opportunity." : "Optimized ML repositioning destination.",
                            
                            // Legacy Support
                            AvgFare = Math.Round(revenueDict.GetValueOrDefault(zone.ZoneId, 0m) / Math.Max(1, demandDict.GetValueOrDefault(zone.ZoneId, 1)), 2),
                            AvgTip = Math.Round((revenueDict.GetValueOrDefault(zone.ZoneId, 0m) * 0.15m) / Math.Max(1, demandDict.GetValueOrDefault(zone.ZoneId, 1)), 2)
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
                        Borough = z.Borough ?? "Unknown",
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

        private async Task<List<(int ZoneId, int PickupCount)>> GetDemandPerZoneNativelyAsync(CancellationToken ct)
        {
            var data = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.DeletedAt == null && t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return data.Select(x => (x.ZoneId, x.Count)).ToList();
        }

        private async Task<List<(int ZoneId, int DriverCount)>> GetDriverSupplyPerZoneNativelyAsync(CancellationToken ct)
        {
            var data = await _unitOfWork.Drivers.Query()
                .AsNoTracking()
                .Where(d => d.Status == CurrentStatus.Available)
                .Select(d => new
                {
                    DriverId = d.UserId,
                    LastTripDropoffZoneId = d.Trips
                        .Where(t => t.DeletedAt == null && t.DropoffLocation != null && t.DropoffLocation.ZoneId != null)
                        .OrderByDescending(t => t.StartedAt)
                        .Select(t => t.DropoffLocation!.ZoneId)
                        .FirstOrDefault()
                })
                .Where(d => d.LastTripDropoffZoneId != null)
                .GroupBy(d => d.LastTripDropoffZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return data.Select(x => (x.ZoneId, x.Count)).ToList();
        }

        private async Task<List<(int ZoneId, decimal Revenue)>> GetRevenuePerZoneNativelyAsync(CancellationToken ct)
        {
            var data = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.DeletedAt == null && t.TotalAmount != null && t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Revenue = g.Sum(t => t.TotalAmount!.Value) })
                .ToListAsync(ct);

            return data.Select(x => (x.ZoneId, x.Revenue)).ToList();
        }

        private async Task<List<(int ZoneId, int ActiveCount)>> GetActiveTripsPerZoneNativelyAsync(CancellationToken ct)
        {
            var data = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.DeletedAt == null && t.ProcessStatus == "Ongoing" && t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            return data.Select(x => (x.ZoneId, x.Count)).ToList();
        }
    }
}
