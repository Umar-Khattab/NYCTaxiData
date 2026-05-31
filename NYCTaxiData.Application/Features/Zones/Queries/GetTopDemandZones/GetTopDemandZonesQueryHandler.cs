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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetTopDemandZones
{
    public class GetTopDemandZonesQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetTopDemandZonesQuery, Result<List<TopDemandZoneDto>>>
    {
        public async Task<Result<List<TopDemandZoneDto>>> Handle(
            GetTopDemandZonesQuery request,
            CancellationToken cancellationToken)
        {
            var limit = request.Limit > 0 ? request.Limit : 10;

            var totalTripsCount = await _unitOfWork.Trips.Query().CountAsync(cancellationToken);

            var dbDemand = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var topDemandZones = new List<TopDemandZoneDto>();

            foreach (var item in dbDemand)
            {
                if (zoneDict.TryGetValue(item.ZoneId, out var zone))
                {
                    double percentage = totalTripsCount > 0
                        ? (double)item.Count / totalTripsCount * 100.0
                        : 0.0;

                    topDemandZones.Add(new TopDemandZoneDto
                    {
                        ZoneId = zone.ZoneId,
                        ZoneName = zone.ZoneName,
                        Borough = zone.Borough ?? "Unknown",
                        PickupCount = item.Count,
                        PercentageOfTotal = Math.Round(percentage, 2)
                    });
                }
            }

            return Result<List<TopDemandZoneDto>>.Success(topDemandZones, "Top demand zones calculated successfully");
        }
    }
}
