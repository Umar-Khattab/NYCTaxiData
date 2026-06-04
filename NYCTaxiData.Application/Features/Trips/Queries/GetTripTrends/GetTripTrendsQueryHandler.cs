using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripTrends
{
    public class GetTripTrendsQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetTripTrendsQuery, Result<List<TripTrendDto>>>
    {
        public async Task<Result<List<TripTrendDto>>> Handle(
            GetTripTrendsQuery request,
            CancellationToken cancellationToken)
        {
            var dailyAggregates = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t =>   t.StartedAt != null)
                .GroupBy(t => t.StartedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(), 
                    Fare = g.Average(t => t.FareAmount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync(cancellationToken);

            var trendList = dailyAggregates.Select(x => new TripTrendDto
            {
                PeriodLabel = x.Date.ToString("yyyy-MM-dd"),
                TripCount = x.Count,
                AverageFare = Math.Round(x.Fare ?? 0, 2)
            }).ToList();

            return Result<List<TripTrendDto>>.Success(trendList, "Trip trends calculated successfully");
        }
    }
}
