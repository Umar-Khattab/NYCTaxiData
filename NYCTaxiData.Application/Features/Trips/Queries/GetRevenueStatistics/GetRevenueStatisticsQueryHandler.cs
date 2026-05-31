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

namespace NYCTaxiData.Application.Features.Trips.Queries.GetRevenueStatistics
{
    public class GetRevenueStatisticsQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetRevenueStatisticsQuery, Result<RevenueStatisticsDto>>
    {
        public async Task<Result<RevenueStatisticsDto>> Handle(
            GetRevenueStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Trips.Query().AsNoTracking()
                .Where(t => t.DeletedAt == null && t.StartedAt != null);

            if (request.StartDate.HasValue)
                query = query.Where(t => t.StartedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(t => t.StartedAt <= request.EndDate.Value);

            var totalTrips = await query.CountAsync(cancellationToken);

            decimal totalRevenue = 0m;
            decimal totalFare = 0m;
            decimal totalTip = 0m;
            decimal avgTipPct = 0m;
            var timeSeries = new List<RevenuePeriodPointDto>();

            if (totalTrips > 0)
            {
                totalRevenue = await query.SumAsync(t => t.TotalAmount ?? 0m, cancellationToken);
                totalFare = await query.SumAsync(t => t.FareAmount, cancellationToken);
                totalTip = await query.SumAsync(t => t.TipAmount ?? 0m, cancellationToken);

                if (totalFare > 0)
                {
                    avgTipPct = (totalTip / totalFare) * 100m;
                }

                // Group by date in DB
                var dailyAggs = await query
                    .GroupBy(t => t.StartedAt!.Value.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Revenue = g.Sum(t => t.TotalAmount ?? 0m),
                        Fare = g.Sum(t => t.FareAmount),
                        Tip = g.Sum(t => t.TipAmount ?? 0m)
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync(cancellationToken);

                timeSeries = dailyAggs.Select(x => new RevenuePeriodPointDto
                {
                    PeriodLabel = x.Date.ToString("yyyy-MM-dd"),
                    Revenue = Math.Round(x.Revenue, 2),
                    FareAmount = Math.Round(x.Fare, 2),
                    TipAmount = Math.Round(x.Tip, 2)
                }).ToList();
            }

            var stats = new RevenueStatisticsDto
            {
                TotalRevenue = Math.Round(totalRevenue, 2),
                TotalFareAmount = Math.Round(totalFare, 2),
                TotalTipAmount = Math.Round(totalTip, 2),
                AvgTipPercentage = Math.Round(avgTipPct, 2),
                TimeSeriesData = timeSeries
            };

            return Result<RevenueStatisticsDto>.Success(stats, "Revenue statistics retrieved successfully");
        }
    }
}
