using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneStatistics
{
    public class GetZoneStatisticsQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetZoneStatisticsQuery, Result<ZoneStatisticsDto>>
    {
        public async Task<Result<ZoneStatisticsDto>> Handle(
            GetZoneStatisticsQuery request,
            CancellationToken cancellationToken)
        {
            string zoneName = "All Zones";
            string borough = "All Boroughs";

            if (request.ZoneId.HasValue)
            {
                var zone = await _unitOfWork.Zones.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(z => z.ZoneId == request.ZoneId.Value, cancellationToken);

                if (zone == null)
                    return Result<ZoneStatisticsDto>.Failure($"Zone with ID {request.ZoneId.Value} not found", "NotFound");

                zoneName = zone.ZoneName;
                borough = zone.Borough ?? "Unknown";
            }

            // Create query filters
            var pickupQuery = _unitOfWork.Trips.Query().AsNoTracking();
            var dropoffQuery = _unitOfWork.Trips.Query().AsNoTracking();

            if (request.ZoneId.HasValue)
            {
                pickupQuery = pickupQuery.Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId == request.ZoneId.Value);
                dropoffQuery = dropoffQuery.Where(t => t.DropoffLocation != null && t.DropoffLocation.ZoneId == request.ZoneId.Value);
            }
            else
            {
                pickupQuery = pickupQuery.Where(t => t.PickupLocationId != null);
                dropoffQuery = dropoffQuery.Where(t => t.DropoffLocationId != null);
            }

            // High-performance count & average aggregates directly on database
            var totalPickupTrips = await pickupQuery.CountAsync(cancellationToken);
            var totalDropoffTrips = await dropoffQuery.CountAsync(cancellationToken);

            decimal totalRevenue = 0m;
            decimal avgFare = 0m;
            decimal avgTip = 0m;
            int busiestHour = 0;
            string busiestDay = "Unknown";

            if (totalPickupTrips > 0)
            {
                totalRevenue = await pickupQuery.SumAsync(t => t.TotalAmount ?? 0m, cancellationToken);
                avgFare = await pickupQuery.AverageAsync(t => t.FareAmount, cancellationToken);
                avgTip = await pickupQuery.AverageAsync(t => t.TipAmount ?? 0m, cancellationToken);

                // Group by hour directly in database
                var busiestHourGroup = await pickupQuery
                    .Where(t => t.StartedAt != null)
                    .GroupBy(t => t.StartedAt!.Value.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync(cancellationToken);

                if (busiestHourGroup != null)
                {
                    busiestHour = busiestHourGroup.Hour;
                }

                // Group by DayOfWeek directly in database
                var dayOfWeekGroup = await pickupQuery
                    .Where(t => t.StartedAt != null)
                    .GroupBy(t => t.StartedAt!.Value.DayOfWeek)
                    .Select(g => new { DayOfWeek = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefaultAsync(cancellationToken);

                if (dayOfWeekGroup != null)
                {
                    busiestDay = dayOfWeekGroup.DayOfWeek.ToString();
                }
            }

            var stats = new ZoneStatisticsDto
            {
                ZoneId = request.ZoneId ?? 0,
                ZoneName = zoneName,
                Borough = borough,
                TotalPickupTrips = totalPickupTrips,
                TotalDropoffTrips = totalDropoffTrips,
                TotalRevenue = Math.Round(totalRevenue, 2),
                AvgFare = Math.Round(avgFare, 2),
                AvgTip = Math.Round(avgTip, 2),
                BusiestHourOfDay = busiestHour,
                BusiestDayOfWeek = busiestDay
            };

            return Result<ZoneStatisticsDto>.Success(stats, "Zone statistics computed successfully");
        }
    }
}
