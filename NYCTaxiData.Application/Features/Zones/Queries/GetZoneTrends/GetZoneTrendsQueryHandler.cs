using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneTrends
{
    public class GetZoneTrendsQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetZoneTrendsQuery, Result<List<ZoneTrendDto>>>
    {
        public async Task<Result<List<ZoneTrendDto>>> Handle(
            GetZoneTrendsQuery request,
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

            var type = request.TrendType.ToLower();
            var trends = new List<ZoneTrendDto>();

            if (type == "daily")
            {
                var dailyAggregates = await query
                    .GroupBy(t => t.StartedAt!.Value.DayOfWeek)
                    .Select(g => new
                    {
                        Day = g.Key,
                        Count = g.Count(), 
                        Fare = g.Average(t => t.FareAmount)
                    })
                    .ToListAsync(cancellationToken);

                trends = dailyAggregates.Select(x => new ZoneTrendDto
                {
                    TimeLabel = x.Day.ToString(),
                    TripCount = x.Count, 
                    AvgFare = Math.Round(x.Fare ?? 0, 2)
                })
                .OrderBy(x => Enum.Parse<DayOfWeek>(x.TimeLabel))
                .ToList();
            }
            else // hourly by default
            {
                var hourlyAggregates = await query
                    .GroupBy(t => t.StartedAt!.Value.Hour)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        Count = g.Count(), 
                        Fare = g.Average(t => t.FareAmount)
                    })
                    .ToListAsync(cancellationToken);

                // Initialize 24 hours to ensure continuous series
                var hourDict = hourlyAggregates.ToDictionary(x => x.Hour, x => x);
                for (int h = 0; h < 24; h++)
                {
                    if (hourDict.TryGetValue(h, out var agg))
                    {
                        trends.Add(new ZoneTrendDto
                        {
                            TimeLabel = $"{h:D2}:00",
                            TripCount = agg.Count, 
                            AvgFare = Math.Round(agg.Fare ?? 0, 2)
                        });
                    }
                    else
                    {
                        trends.Add(new ZoneTrendDto
                        {
                            TimeLabel = $"{h:D2}:00",
                            TripCount = 0,
                            TotalRevenue = 0m,
                            AvgFare = 0m
                        });
                    }
                }
            }

            return Result<List<ZoneTrendDto>>.Success(trends, "Zone trends retrieved successfully");
        }
    }
}
