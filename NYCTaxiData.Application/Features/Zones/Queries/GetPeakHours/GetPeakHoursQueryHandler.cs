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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetPeakHours
{
    public class GetPeakHoursQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetPeakHoursQuery, Result<List<PeakHoursDto>>>
    {
        public async Task<Result<List<PeakHoursDto>>> Handle(
            GetPeakHoursQuery request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Trips.Query().AsNoTracking()
                .Where(t => t.StartedAt != null);

            if (request.ZoneId.HasValue)
            {
                query = query.Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId == request.ZoneId.Value);
            }
            else
            {
                query = query.Where(t => t.PickupLocationId != null);
            }

            var hourlyAggregates = await query
                .GroupBy(t => t.StartedAt!.Value.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    TripCount = g.Count(),
                    Revenue = g.Sum(t => t.TotalAmount ?? 0m),
                    AvgFare = g.Average(t => t.FareAmount)
                })
                .ToListAsync(cancellationToken);

            var peakHoursList = new List<PeakHoursDto>();
            var hourDict = hourlyAggregates.ToDictionary(x => x.Hour, x => x);

            for (int h = 0; h < 24; h++)
            {
                if (hourDict.TryGetValue(h, out var agg))
                {
                    peakHoursList.Add(new PeakHoursDto
                    {
                        Hour = h,
                        TripCount = agg.TripCount,
                        TotalRevenue = Math.Round(agg.Revenue, 2),
                        AverageFare = Math.Round(agg.AvgFare, 2)
                    });
                }
                else
                {
                    peakHoursList.Add(new PeakHoursDto
                    {
                        Hour = h,
                        TripCount = 0,
                        TotalRevenue = 0m,
                        AverageFare = 0m
                    });
                }
            }

            return Result<List<PeakHoursDto>>.Success(peakHoursList, "Peak hours calculated successfully");
        }
    }
}
