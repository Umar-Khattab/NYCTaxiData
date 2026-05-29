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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetTopRevenueZones
{
    public class GetTopRevenueZonesQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetTopRevenueZonesQuery, Result<List<TopRevenueZoneDto>>>
    {
        public async Task<Result<List<TopRevenueZoneDto>>> Handle(
            GetTopRevenueZonesQuery request,
            CancellationToken cancellationToken)
        {
            var limit = request.Limit > 0 ? request.Limit : 10;

            var totalRevenueSum = await _unitOfWork.Trips.Query()
                .Where(t => t.TotalAmount != null)
                .SumAsync(t => t.TotalAmount!.Value, cancellationToken);

            var dbRevenue = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null && t.TotalAmount != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Revenue = g.Sum(t => t.TotalAmount!.Value) })
                .OrderByDescending(x => x.Revenue)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var topRevenueZones = new List<TopRevenueZoneDto>();

            foreach (var item in dbRevenue)
            {
                if (zoneDict.TryGetValue(item.ZoneId, out var zone))
                {
                    double percentage = totalRevenueSum > 0
                        ? (double)(item.Revenue / totalRevenueSum) * 100.0
                        : 0.0;

                    topRevenueZones.Add(new TopRevenueZoneDto
                    {
                        ZoneId = zone.ZoneId,
                        ZoneName = zone.ZoneName,
                        Borough = zone.Borough ?? "Unknown",
                        TotalRevenue = Math.Round(item.Revenue, 2),
                        PercentageOfTotal = Math.Round(percentage, 2)
                    });
                }
            }

            return Result<List<TopRevenueZoneDto>>.Success(topRevenueZones, "Top revenue zones calculated successfully");
        }
    }
}
