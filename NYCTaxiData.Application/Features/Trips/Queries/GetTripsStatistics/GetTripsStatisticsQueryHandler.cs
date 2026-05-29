using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripsStatistics
{
    public class GetTripsStatisticsQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetTripsStatisticsQuery, Result<TripStatisticsDto>>
    {
        public async Task<Result<TripStatisticsDto>> Handle(
            GetTripsStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Trips.Query().AsNoTracking()
                .Where(t => t.DeletedAt == null);

            var totalTrips = await query.CountAsync(cancellationToken);

            var completedTrips = await query
                .CountAsync(t => t.EndedAt != null && t.ProcessStatus == "Completed", cancellationToken);

            var ongoingTrips = totalTrips - completedTrips;

            decimal totalRevenue = 0m;
            decimal avgFare = 0m;
            decimal avgTip = 0m;
            double avgDuration = 0.0;

            if (totalTrips > 0)
            {
                totalRevenue = await query.SumAsync(t => t.TotalAmount ?? 0m, cancellationToken);
                avgFare = await query.AverageAsync(t => t.FareAmount, cancellationToken);
                avgTip = await query.AverageAsync(t => t.TipAmount ?? 0m, cancellationToken);

                // Calculate average duration in minutes for completed trips
                var durations = await query
                    .Where(t => t.StartedAt != null && t.EndedAt != null)
                    .Select(t => new { t.StartedAt, t.EndedAt })
                    .ToListAsync(cancellationToken);

                if (durations.Count > 0)
                {
                    avgDuration = durations.Average(x => (x.EndedAt!.Value - x.StartedAt!.Value).TotalMinutes);
                }
            }

            var stats = new TripStatisticsDto
            {
                TotalTrips = totalTrips,
                CompletedTrips = completedTrips,
                OngoingTrips = ongoingTrips,
                TotalRevenue = Math.Round(totalRevenue, 2),
                AvgFareAmount = Math.Round(avgFare, 2),
                AvgTipAmount = Math.Round(avgTip, 2),
                AverageDurationMinutes = Math.Round(avgDuration, 2)
            };

            return Result<TripStatisticsDto>.Success(stats, "Trip statistics retrieved successfully");
        }
    }
}
