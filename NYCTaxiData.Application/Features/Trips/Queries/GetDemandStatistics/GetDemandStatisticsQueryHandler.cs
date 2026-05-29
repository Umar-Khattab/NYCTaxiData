using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetDemandStatistics
{
    public class GetDemandStatisticsQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetDemandStatisticsQuery, Result<DemandStatisticsDto>>
    {
        public async Task<Result<DemandStatisticsDto>> Handle(
            GetDemandStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Trips.Query().AsNoTracking()
                .Where(t => t.DeletedAt == null && t.StartedAt != null);

            if (request.StartDate.HasValue)
                query = query.Where(t => t.StartedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(t => t.StartedAt <= request.EndDate.Value);

            var totalTrips = await query.CountAsync(cancellationToken);

            string busiestDay = "Unknown";
            int busiestHour = 0;
            var timeSeries = new List<DemandPeriodPointDto>();

            if (totalTrips > 0)
            {
                // Busiest hour
                var hourGroup = await query
                    .GroupBy(t => t.StartedAt!.Value.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync(cancellationToken);

                if (hourGroup != null) busiestHour = hourGroup.Hour;

                // Busiest day
                var dayGroup = await query
                    .GroupBy(t => t.StartedAt!.Value.DayOfWeek)
                    .Select(g => new { Day = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync(cancellationToken);

                if (dayGroup != null) busiestDay = dayGroup.Day.ToString();

                // Group by date
                var dailyAggs = await query
                    .GroupBy(t => t.StartedAt!.Value.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Date)
                    .ToListAsync(cancellationToken);

                timeSeries = dailyAggs.Select(x => new DemandPeriodPointDto
                {
                    PeriodLabel = x.Date.ToString("yyyy-MM-dd"),
                    TripCount = x.Count
                }).ToList();
            }

            var stats = new DemandStatisticsDto
            {
                TotalTrips = totalTrips,
                BusiestDayOfWeek = busiestDay,
                BusiestHourOfDay = busiestHour,
                TimeSeriesData = timeSeries
            };

            return Result<DemandStatisticsDto>.Success(stats, "Demand statistics retrieved successfully");
        }
    }
}
