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

namespace NYCTaxiData.Application.Features.Trips.Queries.GetPeakHours
{
    public class GetPeakHoursQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetPeakHoursQuery, Result<List<TripPeakHoursDto>>>
    {
        public async Task<Result<List<TripPeakHoursDto>>> Handle(
            GetPeakHoursQuery request,
            CancellationToken cancellationToken)
        {
            var hourlyAggregates = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.DeletedAt == null && t.StartedAt != null)
                .GroupBy(t => t.StartedAt!.Value.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    TripCount = g.Count(),
                    Revenue = g.Sum(t => t.TotalAmount ?? 0m)
                })
                .ToListAsync(cancellationToken);

            var peakHoursList = new List<TripPeakHoursDto>();
            var hourDict = hourlyAggregates.ToDictionary(x => x.Hour, x => x);

            for (int h = 0; h < 24; h++)
            {
                if (hourDict.TryGetValue(h, out var agg))
                {
                    peakHoursList.Add(new TripPeakHoursDto
                    {
                        Hour = h,
                        TripCount = agg.TripCount,
                        TotalRevenue = Math.Round(agg.Revenue, 2)
                    });
                }
                else
                {
                    peakHoursList.Add(new TripPeakHoursDto
                    {
                        Hour = h,
                        TripCount = 0,
                        TotalRevenue = 0m
                    });
                }
            }

            return Result<List<TripPeakHoursDto>>.Success(peakHoursList.OrderBy(x => x.Hour).ToList(), "Peak hours calculated successfully");
        }
    }
}
