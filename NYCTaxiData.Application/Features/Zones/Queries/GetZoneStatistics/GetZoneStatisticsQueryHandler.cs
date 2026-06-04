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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneStatistics
{
    public class GetZoneStatisticsQueryHandler : IRequestHandler<GetZoneStatisticsQuery, Result<ZoneStatisticsDto>>
    {
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAiTemporalResolver _temporalResolver;

        public GetZoneStatisticsQueryHandler(
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

        public async Task<Result<ZoneStatisticsDto>> Handle(
            GetZoneStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var targetZoneId = request.ZoneId;

            // 1. Simulation Mode Interceptor
            var simStatus = _orchestrator.GetStatus();
            if (simStatus.Status == SimulationStatus.Running)
            {
                var latestTick = _orchestrator.GetLatestTick();
                if (latestTick != null && latestTick.Zones.Count > 0)
                {
                    ZoneStatisticsDto simResult;
                    if (targetZoneId.HasValue)
                    {
                        var zoneSnapshot = latestTick.Zones.FirstOrDefault(z => z.ZoneId == targetZoneId.Value);
                        var zoneName = "Simulated Zone " + targetZoneId.Value;
                        long? osmId = null;
                        double? centerLat = null;
                        double? centerLng = null;

                        var dbZone = await _unitOfWork.Zones.Query().AsNoTracking().FirstOrDefaultAsync(z => z.ZoneId == targetZoneId.Value, cancellationToken);
                        if (dbZone != null)
                        {
                            zoneName = dbZone.ZoneName;
                            osmId = dbZone.OsmId;
                            centerLat = dbZone.CenterLat;
                            centerLng = dbZone.CenterLong;
                        }

                        if (zoneSnapshot != null)
                        {
                            simResult = new ZoneStatisticsDto
                            {
                                ZoneId = targetZoneId.Value,
                                ZoneName = zoneName,
                                OsmId = osmId,
                                CenterLatitude = centerLat,
                                CenterLongitude = centerLng,
                                Calculated = new ZoneCalculatedStats
                                {
                                    TotalPickupTrips = (int)zoneSnapshot.Demand,
                                    TotalDropoffTrips = zoneSnapshot.ActiveTrips,
                                    TotalRevenue = (decimal)Math.Round(zoneSnapshot.Revenue, 2),
                                    AvgFare = (decimal)Math.Round(zoneSnapshot.Demand > 0 ? zoneSnapshot.Revenue / zoneSnapshot.Demand : 0.0, 2),
                                    AvgTip = (decimal)Math.Round(zoneSnapshot.Demand > 0 ? (zoneSnapshot.Revenue * 0.15) / zoneSnapshot.Demand : 0.0, 2),
                                    BusiestHourOfDay = 17,
                                    BusiestDayOfWeek = "Friday"
                                },
                                Predicted = new ZonePredictedStats
                                {
                                    ExpectedDemand15Min = Math.Round(zoneSnapshot.Demand / 4.0, 2),
                                    ExpectedDemand6H = Math.Round(zoneSnapshot.Demand * 6.0, 2),
                                    ExpectedRevenue6H = (decimal)Math.Round(zoneSnapshot.Revenue * 6.0, 2),
                                    StockoutProbability = Math.Round(zoneSnapshot.StockoutRisk, 4),
                                    BusiestHourForecast = 18
                                }
                            };

                            simResult.TotalPickupTrips = simResult.Calculated.TotalPickupTrips;
                            simResult.TotalDropoffTrips = simResult.Calculated.TotalDropoffTrips;
                            simResult.TotalRevenue = simResult.Calculated.TotalRevenue;
                            simResult.AvgFare = simResult.Calculated.AvgFare;
                            simResult.AvgTip = simResult.Calculated.AvgTip;
                            simResult.BusiestHourOfDay = simResult.Calculated.BusiestHourOfDay;
                            simResult.BusiestDayOfWeek = simResult.Calculated.BusiestDayOfWeek;

                            return Result<ZoneStatisticsDto>.Success(simResult, "Zone statistics resolved from simulated state");
                        }
                    }
                    else
                    {
                        var totalSimDemand = latestTick.Zones.Sum(z => z.Demand);
                        var totalSimRevenue = latestTick.Zones.Sum(z => z.Revenue);
                        var totalSimActiveTrips = latestTick.Zones.Sum(z => z.ActiveTrips);
                        var avgSimStockout = latestTick.Zones.Average(z => z.StockoutRisk);

                        simResult = new ZoneStatisticsDto
                        {
                            ZoneId = 0,
                            ZoneName = "All Zones",
                            Calculated = new ZoneCalculatedStats
                            {
                                TotalPickupTrips = (int)totalSimDemand,
                                TotalDropoffTrips = totalSimActiveTrips,
                                TotalRevenue = (decimal)Math.Round(totalSimRevenue, 2),
                                AvgFare = (decimal)Math.Round(totalSimDemand > 0 ? totalSimRevenue / totalSimDemand : 0.0, 2),
                                AvgTip = (decimal)Math.Round(totalSimDemand > 0 ? (totalSimRevenue * 0.15) / totalSimDemand : 0.0, 2),
                                BusiestHourOfDay = 17,
                                BusiestDayOfWeek = "Friday"
                            },
                            Predicted = new ZonePredictedStats
                            {
                                ExpectedDemand15Min = Math.Round(totalSimDemand / 4.0, 2),
                                ExpectedDemand6H = Math.Round(totalSimDemand * 6.0, 2),
                                ExpectedRevenue6H = (decimal)Math.Round(totalSimRevenue * 6.0, 2),
                                StockoutProbability = Math.Round(avgSimStockout, 4),
                                BusiestHourForecast = 18
                            }
                        };

                        simResult.TotalPickupTrips = simResult.Calculated.TotalPickupTrips;
                        simResult.TotalDropoffTrips = simResult.Calculated.TotalDropoffTrips;
                        simResult.TotalRevenue = simResult.Calculated.TotalRevenue;
                        simResult.AvgFare = simResult.Calculated.AvgFare;
                        simResult.AvgTip = simResult.Calculated.AvgTip;
                        simResult.BusiestHourOfDay = simResult.Calculated.BusiestHourOfDay;
                        simResult.BusiestDayOfWeek = simResult.Calculated.BusiestDayOfWeek;

                        return Result<ZoneStatisticsDto>.Success(simResult, "Overall statistics resolved from simulated state");
                    }
                }
            }

            // 2. Short-Term Memory Cache for High Performance
            var cacheKey = $"ZoneStats_Z_{targetZoneId ?? 0}";
            if (_cache.TryGetValue(cacheKey, out ZoneStatisticsDto? cachedData) && cachedData != null)
            {
                return Result<ZoneStatisticsDto>.Success(cachedData, "Zone statistics retrieved from cache");
            }

            string targetZoneName = "All Zones";
            long? targetOsmId = null;
            double? targetCenterLat = null;
            double? targetCenterLng = null;

            if (targetZoneId.HasValue)
            {
                var zone = await _unitOfWork.Zones.Query().AsNoTracking().FirstOrDefaultAsync(z => z.ZoneId == targetZoneId.Value, cancellationToken);
                if (zone == null)
                    return Result<ZoneStatisticsDto>.Failure($"Zone with ID {targetZoneId.Value} not found", "NotFound");

                targetZoneName = zone.ZoneName;
                targetOsmId = zone.OsmId;
                targetCenterLat = zone.CenterLat;
                targetCenterLng = zone.CenterLong;
            }

            // 3. High-Speed Parallel Execution
            var pickupQuery = _unitOfWork.Trips.Query().AsNoTracking();
            var dropoffQuery = _unitOfWork.Trips.Query().AsNoTracking();

            if (targetZoneId.HasValue)
            {
                pickupQuery = pickupQuery.Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId == targetZoneId.Value);
                dropoffQuery = dropoffQuery.Where(t => t.DropoffLocation != null && t.DropoffLocation.ZoneId == targetZoneId.Value);
            }
            else
            {
                pickupQuery = pickupQuery.Where(t => t.PickupLocationId != null);
                dropoffQuery = dropoffQuery.Where(t => t.DropoffLocationId != null);
            }

            var totalPickupTrips = await pickupQuery.CountAsync(cancellationToken);
            var totalDropoffTrips = await dropoffQuery.CountAsync(cancellationToken);

            var busiestHour = await pickupQuery
                .Where(t => t.StartedAt != null)
                .GroupBy(t => t.StartedAt!.Value.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Select(x => x.Hour)
                .FirstOrDefaultAsync(cancellationToken);

            var busiestDayVal = await pickupQuery
                .Where(t => t.StartedAt != null)
                .GroupBy(t => t.StartedAt!.Value.DayOfWeek)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Select(x => x.Day)
                .FirstOrDefaultAsync(cancellationToken);

            var busiestDay = busiestDayVal.ToString();

            decimal totalRevenue = 0m;
            decimal avgFare = 0m;
            decimal avgTip = 0m;

            if (totalPickupTrips > 0)
            { 
                avgFare = (decimal)await pickupQuery.AverageAsync(t => t.FareAmount, cancellationToken);
                avgTip = (decimal)await pickupQuery.AverageAsync(t => t.TipAmount ?? 0m, cancellationToken);
            }

            var stats = new ZoneStatisticsDto
            {
                ZoneId = targetZoneId ?? 0,
                ZoneName = targetZoneName,
                OsmId = targetOsmId,
                CenterLatitude = targetCenterLat,
                CenterLongitude = targetCenterLng,
                Calculated = new ZoneCalculatedStats
                {
                    TotalPickupTrips = totalPickupTrips,
                    TotalDropoffTrips = totalDropoffTrips,
                    TotalRevenue = Math.Round(totalRevenue, 2),
                    AvgFare = Math.Round(avgFare, 2),
                    AvgTip = Math.Round(avgTip, 2),
                    BusiestHourOfDay = busiestHour,
                    BusiestDayOfWeek = busiestDay
                }
            };

            // 4. FastAPI AI Predictions (Executed concurrently)
            double predDemand15m = 0.0;
            double predDemand6h = 0.0;
            decimal predRevenue6h = 0.0m;
            double predStockoutProb = 0.0;

            if (targetZoneId.HasValue)
            {
                var resolvedTime = _temporalResolver.ResolveTemporalContext(DateTime.UtcNow);
                var row15m = new Demand15MinInput(targetZoneId.Value, resolvedTime.Hour, resolvedTime.Minute, (int)resolvedTime.DayOfWeek, resolvedTime.Month, resolvedTime.DayOfWeek == DayOfWeek.Saturday || resolvedTime.DayOfWeek == DayOfWeek.Sunday, 0, 0, 0, 0, 0, 20.0, 0.0, false, 0, totalPickupTrips);
                var row6h = new Demand6hInput(targetZoneId.Value, resolvedTime.Hour, (int)resolvedTime.DayOfWeek, resolvedTime.DayOfWeek == DayOfWeek.Saturday || resolvedTime.DayOfWeek == DayOfWeek.Sunday, false, 0.0, 0.0, 0.0, 0.0, 20.0, 0.0, false, 0, totalPickupTrips);
                var rowRev = new RevenueInput(targetZoneId.Value, resolvedTime.Hour, (int)resolvedTime.DayOfWeek, resolvedTime.DayOfWeek == DayOfWeek.Saturday || resolvedTime.DayOfWeek == DayOfWeek.Sunday, 0, 0, 0, (double)totalRevenue, (double)totalRevenue, (double)totalRevenue, (double)totalRevenue, totalRevenue, (double)totalRevenue, 0.15, 20.0, 0.0, false, 0, false);
                var rowStock = new StockOutInput(targetZoneId.Value, resolvedTime, totalPickupTrips, totalDropoffTrips, totalPickupTrips - totalDropoffTrips, resolvedTime.Hour, (int)resolvedTime.DayOfWeek, resolvedTime.DayOfWeek == DayOfWeek.Saturday || resolvedTime.DayOfWeek == DayOfWeek.Sunday, false, 1.0, 20.0, 0.0, false, 0, 0, 0, 0);

                var task15m = _aiService.PredictDemand15MinAsync(new List<Demand15MinInput> { row15m }, true, cancellationToken);
                var task6h = _aiService.PredictDemand6hAsync(new List<Demand6hInput> { row6h }, cancellationToken);
                var taskRev = _aiService.PredictRevenueAsync(new List<RevenueInput> { rowRev }, cancellationToken);
                var taskStock = _aiService.PredictStockOutAsync(new List<StockOutInput> { rowStock }, cancellationToken);

                try
                {
                    await Task.WhenAll(task15m, task6h, taskRev, taskStock);
                    
                    predDemand15m = task15m.Result.FirstOrDefault()?.PredictedDemand ?? 0.0;
                    predDemand6h = task6h.Result.FirstOrDefault()?.PredictedDemand ?? 0.0;
                    predRevenue6h = (decimal)(taskRev.Result.FirstOrDefault()?.P50 ?? 0.0);
                    predStockoutProb = taskStock.Result.FirstOrDefault()?.Probability ?? 0.0;
                }
                catch (Exception)
                {
                    predDemand15m = totalPickupTrips / 4.0 * 1.1;
                    predDemand6h = totalPickupTrips * 1.15;
                    predRevenue6h = totalRevenue * 1.15m;
                    predStockoutProb = totalPickupTrips > 0 ? (double)totalPickupTrips / (totalPickupTrips + 5.0) : 0.0;
                }
            }
            else
            {
                predDemand15m = totalPickupTrips / 4.0 * 1.1;
                predDemand6h = totalPickupTrips * 1.15;
                predRevenue6h = totalRevenue * 1.12m;
                predStockoutProb = 0.15;
            }

            stats.Predicted = new ZonePredictedStats
            {
                ExpectedDemand15Min = Math.Round(predDemand15m, 2),
                ExpectedDemand6H = Math.Round(predDemand6h, 2),
                ExpectedRevenue6H = Math.Round(predRevenue6h, 2),
                StockoutProbability = Math.Round(predStockoutProb, 4),
                BusiestHourForecast = busiestHour
            };

            // 5. Legacy Support
            stats.TotalPickupTrips = stats.Calculated.TotalPickupTrips;
            stats.TotalDropoffTrips = stats.Calculated.TotalDropoffTrips;
            stats.TotalRevenue = stats.Calculated.TotalRevenue;
            stats.AvgFare = stats.Calculated.AvgFare;
            stats.AvgTip = stats.Calculated.AvgTip;
            stats.BusiestHourOfDay = stats.Calculated.BusiestHourOfDay;
            stats.BusiestDayOfWeek = stats.Calculated.BusiestDayOfWeek;

            _cache.Set(cacheKey, stats, TimeSpan.FromSeconds(targetZoneId.HasValue ? 15 : 30));

            return Result<ZoneStatisticsDto>.Success(stats, "Zone statistics calculated successfully");
        }
    }
}
