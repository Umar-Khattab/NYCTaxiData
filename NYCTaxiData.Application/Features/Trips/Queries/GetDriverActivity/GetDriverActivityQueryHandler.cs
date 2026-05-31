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

namespace NYCTaxiData.Application.Features.Trips.Queries.GetDriverActivity
{
    public class GetDriverActivityQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetDriverActivityQuery, Result<List<DriverActivityDto>>>
    {
        public async Task<Result<List<DriverActivityDto>>> Handle(
            GetDriverActivityQuery request,
            CancellationToken cancellationToken)
        {
            // Group trip earnings and count by driver on database
            var driverAggregates = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.DeletedAt == null && t.DriverId != null && t.TotalAmount != null)
                .GroupBy(t => t.DriverId!.Value)
                .Select(g => new
                {
                    DriverId = g.Key,
                    Count = g.Count(),
                    Earnings = g.Sum(t => t.TotalAmount!.Value)
                })
                .ToListAsync(cancellationToken);

            var aggDict = driverAggregates.ToDictionary(a => a.DriverId, a => a);

            // Fetch drivers
            var drivers = await _unitOfWork.Drivers.Query()
                .AsNoTracking()
                .Include(d => d.User)
                .ToListAsync(cancellationToken);

            var driverActivity = new List<DriverActivityDto>();

            foreach (var driver in drivers)
            {
                int totalTrips = 0;
                decimal earnings = 0m;

                if (aggDict.TryGetValue(driver.UserId, out var agg))
                {
                    totalTrips = agg.Count;
                    earnings = agg.Earnings;
                }

                driverActivity.Add(new DriverActivityDto
                {
                    DriverId = driver.UserId,
                    DriverName = driver.FullName ?? (driver.User != null ? $"{driver.User.FirstName} {driver.User.LastName}" : "Unknown Driver"),
                    TotalTrips = totalTrips,
                    TotalEarnings = Math.Round(earnings, 2),
                    AverageRating = driver.Rating ?? 0m,
                    CurrentStatus = driver.Status.ToString()
                });
            }

            var orderedActivity = driverActivity
                .OrderByDescending(x => x.TotalEarnings)
                .ToList();

            return Result<List<DriverActivityDto>>.Success(orderedActivity, "Driver activity reports computed successfully");
        }
    }
}
