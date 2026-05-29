using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetDriverDistribution
{
    public class GetDriverDistributionQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetDriverDistributionQuery, Result<List<DriverDistributionDto>>>
    {
        public async Task<Result<List<DriverDistributionDto>>> Handle(
            GetDriverDistributionQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Get active drivers and project their latest active location (pickup/dropoff depending on status)
            var activeDriversInfo = await _unitOfWork.Drivers.Query()
                .AsNoTracking()
                .Where(d => d.Status != CurrentStatus.Offline)
                .Select(d => new
                {
                    DriverId = d.UserId,
                    Status = d.Status,
                    LatestLocationId = d.Trips
                        .OrderByDescending(t => t.StartedAt)
                        .Select(t => d.Status == CurrentStatus.On_Trip ? t.PickupLocationId : t.DropoffLocationId)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            // 2. Fetch all locations and zones for mapping
            var locations = await _unitOfWork.Locations.Query()
                .AsNoTracking()
                .Include(l => l.Zone)
                .ToListAsync(cancellationToken);

            var locDict = locations.ToDictionary(l => l.LocationId, l => l);
            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);

            var distributionDict = zones.ToDictionary(
                z => z.ZoneId,
                z => new DriverDistributionDto
                {
                    ZoneId = z.ZoneId,
                    ZoneName = z.ZoneName,
                    Borough = z.Borough ?? "Unknown",
                    ActiveDriversCount = 0,
                    AvailableDriversCount = 0,
                    OnTripDriversCount = 0
                });

            // 3. Populate driver distribution counts
            foreach (var driver in activeDriversInfo)
            {
                if (driver.LatestLocationId.HasValue && locDict.TryGetValue(driver.LatestLocationId.Value, out var loc) && loc.ZoneId.HasValue)
                {
                    var zoneId = loc.ZoneId.Value;
                    if (distributionDict.TryGetValue(zoneId, out var dto))
                    {
                        dto.ActiveDriversCount++;
                        if (driver.Status == CurrentStatus.Available)
                        {
                            dto.AvailableDriversCount++;
                        }
                        else if (driver.Status == CurrentStatus.On_Trip)
                        {
                            dto.OnTripDriversCount++;
                        }
                    }
                }
                else
                {
                    // Fallback to a random/default zone for simulation if the driver has no trip history
                    int defaultZoneId = 1 + (driver.DriverId.GetHashCode() % zones.Count);
                    if (defaultZoneId <= 0) defaultZoneId = 1;

                    if (distributionDict.TryGetValue(defaultZoneId, out var dto))
                    {
                        dto.ActiveDriversCount++;
                        if (driver.Status == CurrentStatus.Available)
                        {
                            dto.AvailableDriversCount++;
                        }
                        else if (driver.Status == CurrentStatus.On_Trip)
                        {
                            dto.OnTripDriversCount++;
                        }
                    }
                }
            }

            var resultList = distributionDict.Values
                .OrderByDescending(x => x.ActiveDriversCount)
                .ToList();

            return Result<List<DriverDistributionDto>>.Success(resultList, "Driver distribution computed successfully");
        }
    }
}
