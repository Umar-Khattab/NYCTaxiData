using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetPeakHours
{
    public class GetPeakHoursQueryHandler : IRequestHandler<GetPeakHoursQuery, Result<List<TripPeakHoursDto>>>
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

        public async Task<Result<List<TripPeakHoursDto>>> Handle(
            GetPeakHoursQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Simulation Mode Interceptor
            var simStatus = _orchestrator.GetStatus();
            if (simStatus.Status == SimulationStatus.Running)
            {
                var simResult = new List<TripPeakHoursDto>();
                
                var latestTick = _orchestrator.GetLatestTick();
                if (latestTick != null && latestTick.Zones.Count > 0)
                {
                    double totalSimDemand = latestTick.Zones.Sum(z => z.Demand);
                    double totalSimRevenue = latestTick.Zones.Sum(z => z.Revenue);

                    for (int h = 0; h < 24; h++)
                    {
                        double mockDemand = (totalSimDemand / 24.0) * (1.0 + Math.Sin((h - 6) / 24.0 * 2.0 * Math.PI) * 0.4);
                        if (mockDemand < 0) mockDemand = 0;
                        double mockRev = mockDemand * 14.5;

                        simResult.Add(new TripPeakHoursDto
                        {
                            Hour = h,
                            CalculatedTripCount = (int)mockDemand,
                            CalculatedTotalRevenue = (decimal)Math.Round(mockRev, 2),
                            PredictedTripCount = Math.Round(mockDemand * 1.15, 2),
                            PredictedTotalRevenue = (decimal)Math.Round(mockRev * 1.15, 2),

                            // Legacy Support
                            TripCount = (int)mockDemand,
                            TotalRevenue = (decimal)Math.Round(mockRev, 2)
                        });
                    }

                    return Result<List<TripPeakHoursDto>>.Success(simResult.OrderBy(x => x.Hour).ToList(), "Peak hours resolved from simulated state");
                }
            }

            // 2. Short-Term Memory Cache for High Performance
            var cacheKey = "TripsOverallPeakHours";
            if (_cache.TryGetValue(cacheKey, out List<TripPeakHoursDto>? cachedData) && cachedData != null)
            {
                return Result<List<TripPeakHoursDto>>.Success(cachedData, "Peak hours retrieved from cache");
            }

            // 3. High-Speed Parallel Execution
            var dbPeakHoursTask = _unitOfWork.Trips.Query().AsNoTracking()
                .Where(t =>   t.StartedAt != null)
                .GroupBy(t => t.StartedAt!.Value.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    TripCount = g.Count(),
                })
                .ToListAsync(cancellationToken);

            var batchDemand6hInputs = Enumerable.Range(0, 24).Select(h => new Demand6hInput(
                1, h, (int)DateTime.UtcNow.DayOfWeek,
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
                predictions = dbPeakHours.Select(x => new Demand6hResult(1, x.TripCount * 1.1)).ToList();
            }

            var hourDbDict = dbPeakHours.ToDictionary(x => x.Hour, x => x);
            var hourPredDict = predictions.Select((p, idx) => new { Hour = idx, Demand = p.PredictedDemand }).ToDictionary(x => x.Hour, x => x.Demand);

            var peakHoursList = new List<TripPeakHoursDto>();

            for (int h = 0; h < 24; h++)
            {
                int tripCount = 0;
                decimal totalRevenue = 0m;
                decimal avgFare = 14.50m;

                if (hourDbDict.TryGetValue(h, out var agg))
                {
                    tripCount = agg.TripCount; 
                    if (tripCount > 0)
                        avgFare = totalRevenue / tripCount;
                }

                double predCount = hourPredDict.GetValueOrDefault(h, (double)tripCount * 1.15);
                decimal predRev = (decimal)predCount * avgFare;

                peakHoursList.Add(new TripPeakHoursDto
                {
                    Hour = h,
                    CalculatedTripCount = tripCount,
                    CalculatedTotalRevenue = Math.Round(totalRevenue, 2),
                    PredictedTripCount = Math.Round(predCount, 2),
                    PredictedTotalRevenue = Math.Round(predRev, 2),

                    // Legacy Support
                    TripCount = tripCount,
                    TotalRevenue = Math.Round(totalRevenue, 2)
                });
            }

            _cache.Set(cacheKey, peakHoursList, TimeSpan.FromSeconds(15));

            return Result<List<TripPeakHoursDto>>.Success(peakHoursList.OrderBy(x => x.Hour).ToList(), "Peak hours calculated successfully");
        }
    }
}
