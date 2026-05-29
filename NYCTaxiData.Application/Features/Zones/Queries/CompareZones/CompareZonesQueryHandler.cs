using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.CompareZones
{
    public class CompareZonesQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<CompareZonesQuery, Result<ZoneComparisonDto>>
    {
        public async Task<Result<ZoneComparisonDto>> Handle(
            CompareZonesQuery request,
            CancellationToken cancellationToken)
        {
            if (request.ZoneIds == null || request.ZoneIds.Count == 0)
                return Result<ZoneComparisonDto>.Failure("Zone IDs must be provided for comparison", "Validation");

            var comparisonStats = new List<ZoneStatisticsDto>();

            foreach (var zoneId in request.ZoneIds)
            {
                var zone = await _unitOfWork.Zones.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(z => z.ZoneId == zoneId, cancellationToken);

                if (zone == null)
                    continue;

                // Pickup and dropoff queries
                var pickupQuery = _unitOfWork.Trips.Query().AsNoTracking()
                    .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId == zoneId);
                var dropoffQuery = _unitOfWork.Trips.Query().AsNoTracking()
                    .Where(t => t.DropoffLocation != null && t.DropoffLocation.ZoneId == zoneId);

                var totalPickups = await pickupQuery.CountAsync(cancellationToken);
                var totalDropoffs = await dropoffQuery.CountAsync(cancellationToken);

                decimal totalRevenue = 0m;
                decimal avgFare = 0m;
                decimal avgTip = 0m;
                int busiestHour = 0;
                string busiestDay = "Unknown";

                if (totalPickups > 0)
                {
                    totalRevenue = await pickupQuery.SumAsync(t => t.TotalAmount ?? 0m, cancellationToken);
                    avgFare = await pickupQuery.AverageAsync(t => t.FareAmount, cancellationToken);
                    avgTip = await pickupQuery.AverageAsync(t => t.TipAmount ?? 0m, cancellationToken);

                    var hourGroup = await pickupQuery
                        .Where(t => t.StartedAt != null)
                        .GroupBy(t => t.StartedAt!.Value.Hour)
                        .Select(g => new { Hour = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (hourGroup != null)
                        busiestHour = hourGroup.Hour;

                    var dayGroup = await pickupQuery
                        .Where(t => t.StartedAt != null)
                        .GroupBy(t => t.StartedAt!.Value.DayOfWeek)
                        .Select(g => new { Day = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (dayGroup != null)
                        busiestDay = dayGroup.Day.ToString();
                }

                comparisonStats.Add(new ZoneStatisticsDto
                {
                    ZoneId = zoneId,
                    ZoneName = zone.ZoneName,
                    Borough = zone.Borough,
                    TotalPickupTrips = totalPickups,
                    TotalDropoffTrips = totalDropoffs,
                    TotalRevenue = Math.Round(totalRevenue, 2),
                    AvgFare = Math.Round(avgFare, 2),
                    AvgTip = Math.Round(avgTip, 2),
                    BusiestHourOfDay = busiestHour,
                    BusiestDayOfWeek = busiestDay
                });
            }

            if (comparisonStats.Count == 0)
                return Result<ZoneComparisonDto>.Failure("None of the specified zones were found", "NotFound");

            var winnerByRevenue = comparisonStats.OrderByDescending(x => x.TotalRevenue).First();
            var winnerByTrips = comparisonStats.OrderByDescending(x => x.TotalPickupTrips).First();

            var comparisonResult = new ZoneComparisonDto
            {
                ComparisonData = comparisonStats,
                HighestRevenueZone = winnerByRevenue.ZoneName,
                BusiestZone = winnerByTrips.ZoneName
            };

            return Result<ZoneComparisonDto>.Success(comparisonResult, "Zone comparison completed successfully");
        }
    }
}
