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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetPeakHours
{
    public class GetPeakHoursQueryHandler : IRequestHandler<GetPeakHoursQuery, Result<List<PeakHoursDto>>>
    {
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public GetPeakHoursQueryHandler(
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

        public async Task<Result<List<PeakHoursDto>>> Handle(
            GetPeakHoursQuery request,
            CancellationToken cancellationToken)
        {
            var targetZoneId = request.ZoneId;

            // 1. Simulation Mode Interceptor
            var simStatus = _orchestrator.GetStatus();
            if (simStatus.Status == SimulationStatus.Running)
            {
                var simResult = new List<PeakHoursDto>();
                
                if (targetZoneId.HasValue)
                {
                    var zoneHistory = _orchestrator.GetZoneHistory(targetZoneId.Value);
                    if (zoneHistory != null && zoneHistory.Points.Count > 0)
                    {
                        var groups = zoneHistory.Points
                            .GroupBy(p => p.SimulatedTime.Hour)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        for (int h = 0; h < 24; h++)
                        {
                            int tripCount = 0;
                            double revenue = 0.0;
                            double avgFare = 0.0;

                            if (groups.TryGetValue(h, out var points))
                            {
                                tripCount = (int)points.Average(p => p.Demand);
                                revenue = points.Average(p => p.Revenue);
                                avgFare = tripCount > 0 ? revenue / tripCount : 0.0;
                            }

                            simResult.Add(new PeakHoursDto
                            {
                                Hour = h,
                                CalculatedTripCount = tripCount,
                                CalculatedTotalRevenue = (decimal)Math.Round(revenue, 2),
                                CalculatedAverageFare = (decimal)Math.Round(avgFare, 2),
                                PredictedTripCount = Math.Round(tripCount * 1.12, 2),
                                PredictedTotalRevenue = (decimal)Math.Round(revenue * 1.12, 2),

                                // Legacy Support
                                TripCount = tripCount,
                                TotalRevenue = (decimal)Math.Round(revenue, 2),
                                AverageFare = (decimal)Math.Round(avgFare, 2)
                            });
                        }

                        return Result<List<PeakHoursDto>>.Success(simResult.OrderBy(x => x.Hour).ToList(), "Peak hours resolved from simulated zone history");
                    }
                }

                for (int h = 0; h < 24; h++)
                {
                    double mockDemand = 25.0 + Math.Sin((h - 6) / 24.0 * 2.0 * Math.PI) * 15.0;
                    if (mockDemand < 0) mockDemand = 0;
                    double mockRev = mockDemand * 14.5;

                    simResult.Add(new PeakHoursDto
                    {
                        Hour = h,
                        CalculatedTripCount = (int)mockDemand,
                        CalculatedTotalRevenue = (decimal)Math.Round(mockRev, 2),
                        CalculatedAverageFare = 14.50m,
                        PredictedTripCount = Math.Round(mockDemand * 1.15, 2),
                        PredictedTotalRevenue = (decimal)Math.Round(mockRev * 1.15, 2),

                        // Legacy Support
                        TripCount = (int)mockDemand,
                        TotalRevenue = (decimal)Math.Round(mockRev, 2),
                        AverageFare = 14.50m
                    });
                }

                return Result<List<PeakHoursDto>>.Success(simResult.OrderBy(x => x.Hour).ToList(), "Peak hours resolved from simulated state");
            }

            // 2. Short-Term Memory Cache for High Performance
            var cacheKey = $"ZonePeakHours_Z_{targetZoneId ?? 0}";
            if (_cache.TryGetValue(cacheKey, out List<PeakHoursDto>? cachedData) && cachedData != null)
            {
                return Result<List<PeakHoursDto>>.Success(cachedData, "Peak hours retrieved from cache");
            }

            // 3. High-Speed Parallel Execution
            var query = _unitOfWork.Trips.Query().AsNoTracking()
                .Where(t => t.StartedAt != null);

            if (targetZoneId.HasValue)
            {
                query = query.Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId == targetZoneId.Value);
            }
            else
            {
                query = query.Where(t => t.PickupLocationId != null);
            }

            var dbPeakHoursTask = query
                .GroupBy(t => t.StartedAt!.Value.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    TripCount = g.Count(),
                    Revenue = g.Sum(t => t.TotalAmount ?? 0m),
                    AvgFare = g.Average(t => t.FareAmount)
                })
                .ToListAsync(cancellationToken);

            var batchDemand6hInputs = Enumerable.Range(0, 24).Select(h => new Demand6hInput(
                targetZoneId ?? 1, h, (int)DateTime.UtcNow.DayOfWeek,
                DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday,
                false, 0.0, 0.0, 0.0, 0.0, 20.0, 0.0, false, 0, 0
            )).ToList();

            var predictionsTask = _aiService.PredictDemand6hAsync(batchDemand6hInputs, cancellationToken);

            await Task.WhenAll(dbPeakHoursTask, predictionsTask);

            var dbPeakHours = dbPeakHoursTask.Result;
            var predictions = new List<Demand6hResult>();
            try
            {
                predictions = predictionsTask.Result;
            }
            catch (Exception)
            {
                predictions = dbPeakHours.Select(x => new Demand6hResult(targetZoneId ?? 1, x.TripCount * 1.1)).ToList();
            }

            var hourDbDict = dbPeakHours.ToDictionary(x => x.Hour, x => x);
            var hourPredDict = predictions.Select((p, idx) => new { Hour = idx, Demand = p.PredictedDemand }).ToDictionary(x => x.Hour, x => x.Demand);

            var peakHoursList = new List<PeakHoursDto>();

            for (int h = 0; h < 24; h++)
            {
                int tripCount = 0;
                decimal totalRevenue = 0m;
                decimal avgFare = 0m;

                if (hourDbDict.TryGetValue(h, out var agg))
                {
                    tripCount = agg.TripCount;
                    totalRevenue = (decimal)agg.Revenue;
                    avgFare = (decimal)agg.AvgFare;
                }

                double predCount = hourPredDict.GetValueOrDefault(h, (double)tripCount * 1.15);
                decimal predRev = (decimal)predCount * (avgFare > 0 ? avgFare : 14.50m);

                peakHoursList.Add(new PeakHoursDto
                {
                    Hour = h,
                    CalculatedTripCount = tripCount,
                    CalculatedTotalRevenue = Math.Round(totalRevenue, 2),
                    CalculatedAverageFare = Math.Round(avgFare, 2),
                    PredictedTripCount = Math.Round(predCount, 2),
                    PredictedTotalRevenue = Math.Round(predRev, 2),

                    // Legacy Support
                    TripCount = tripCount,
                    TotalRevenue = Math.Round(totalRevenue, 2),
                    AverageFare = Math.Round(avgFare, 2)
                });
            }

            _cache.Set(cacheKey, peakHoursList, TimeSpan.FromSeconds(15));

            return Result<List<PeakHoursDto>>.Success(peakHoursList.OrderBy(x => x.Hour).ToList(), "Peak hours calculated successfully");
        }
    }
}
