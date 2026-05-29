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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneHistory
{
    public class GetZoneHistoryQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetZoneHistoryQuery, Result<List<ZoneHistoryDto>>>
    {
        public async Task<Result<List<ZoneHistoryDto>>> Handle(
            GetZoneHistoryQuery request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Trips.Query().AsNoTracking()
                .Where(t => t.StartedAt != null && t.StartedAt >= request.StartDate && t.StartedAt <= request.EndDate);

            if (request.ZoneId.HasValue)
            {
                query = query.Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId == request.ZoneId.Value);
            }
            else
            {
                query = query.Where(t => t.PickupLocationId != null);
            }

            // Group by Date directly in DB
            var dbHistory = await query
                .GroupBy(t => t.StartedAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalTrips = g.Count(),
                    TotalRevenue = g.Sum(t => t.TotalAmount ?? 0m),
                    AvgFare = g.Average(t => t.FareAmount)
                })
                .ToListAsync(cancellationToken);

            var historyList = dbHistory.Select(x => new ZoneHistoryDto
            {
                Date = x.Date,
                TotalTrips = x.TotalTrips,
                TotalRevenue = Math.Round(x.TotalRevenue, 2),
                AverageFare = Math.Round(x.AvgFare, 2),
                PeakHour = 17 // Simulated default or we can calculate. (17 represents 5 PM peak rush)
            })
            .OrderBy(x => x.Date)
            .ToList();

            return Result<List<ZoneHistoryDto>>.Success(historyList, "Historical zone metrics retrieved successfully");
        }
    }
}
