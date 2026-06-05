using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetDriverDistribution
{
    public class GetDriverDistributionQueryHandler(IUnitOfWork _unitOfWork, IMemoryCache _cache)
        : IRequestHandler<GetDriverDistributionQuery, Result<List<DriverDistributionDto>>>
    {
        public async Task<Result<List<DriverDistributionDto>>> Handle(
            GetDriverDistributionQuery request,
            CancellationToken cancellationToken)
        {
            var recentTime = DateTime.UtcNow.AddHours(-24);
            var onTripStatusStr = CurrentStatus.On_Trip.ToString();

            // 1. Get active drivers and their latest trips
            var activeDrivers = await _unitOfWork.Drivers.Query()
                .AsNoTracking()
                .Where(d => d.Status != CurrentStatus.Offline.ToString())
                .Select(d => new { d.UserId, d.Status })
                .ToListAsync(cancellationToken);

            var activeDriverIds = activeDrivers.Select(d => d.UserId).ToList();

            var trips = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.StartedAt >= recentTime && t.DriverId != null && activeDriverIds.Contains(t.DriverId.Value))
                .Select(t => new { t.DriverId, t.StartedAt, t.PickupLocationId, t.DropoffLocationId })
                .ToListAsync(cancellationToken);

            var latestTripDict = trips
                .GroupBy(t => t.DriverId!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => {
                        var latest = g.OrderByDescending(t => t.StartedAt).First();
                        return new { latest.PickupLocationId, latest.DropoffLocationId };
                    });

            var activeDriversInfo = activeDrivers.Select(d => {
                int? latestLocationId = null;
                if (latestTripDict.TryGetValue(d.UserId, out var trip))
                {
                    latestLocationId = d.Status == onTripStatusStr ? trip.PickupLocationId : trip.DropoffLocationId;
                }
                return new { DriverId = d.UserId, Status = d.Status, LatestLocationId = latestLocationId };
            }).ToList();

            // 2. Fetch zones and location-to-zone mapping (cached for 1 hour)
            const string zonesCacheKey = "ZoneDistribution_ZonesList";
            const string locZoneMapCacheKey = "ZoneDistribution_LocationZoneMap";

            if (!_cache.TryGetValue(zonesCacheKey, out List<Zone>? zones) || zones == null ||
                !_cache.TryGetValue(locZoneMapCacheKey, out Dictionary<int, int>? locationZoneMap) || locationZoneMap == null)
            {
                zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
                var locationsList = await _unitOfWork.Locations.Query()
                    .AsNoTracking()
                    .Where(l => l.ZoneId != null)
                    .Select(l => new { l.LocationId, ZoneId = l.ZoneId!.Value })
                    .ToListAsync(cancellationToken);
                
                locationZoneMap = locationsList.ToDictionary(l => l.LocationId, l => l.ZoneId);

                _cache.Set(zonesCacheKey, zones, TimeSpan.FromHours(1));
                _cache.Set(locZoneMapCacheKey, locationZoneMap, TimeSpan.FromHours(1));
            }

            var distributionDict = zones.ToDictionary(
                z => z.ZoneId,
                z => new DriverDistributionDto
                {
                    ZoneId = z.ZoneId,
                    ZoneName = z.ZoneName,
                    CenterLatitude = z.CenterLat,
                    CenterLongitude = z.CenterLong,
                    OsmId = z.OsmId,
                    ActiveDriversCount = 0,
                    AvailableDriversCount = 0,
                    OnTripDriversCount = 0
                });

            // 3. Populate driver distribution counts
            foreach (var driver in activeDriversInfo)
            {
                if (driver.LatestLocationId.HasValue && locationZoneMap.TryGetValue(driver.LatestLocationId.Value, out var zoneId))
                {
                    if (distributionDict.TryGetValue(zoneId, out var dto))
                    {
                        dto.ActiveDriversCount++;
                        if (driver.Status == CurrentStatus.Available.ToString())
                        {
                            dto.AvailableDriversCount++;
                        }
                        else if (driver.Status == onTripStatusStr)
                        {
                            dto.OnTripDriversCount++;
                        }
                    }
                }
                else
                {
                    // Fallback to a random/default zone for simulation if the driver has no trip history
                    int defaultZoneId = 1 + (Math.Abs(driver.DriverId.GetHashCode()) % zones.Count);
                    if (defaultZoneId <= 0) defaultZoneId = 1;

                    if (distributionDict.TryGetValue(defaultZoneId, out var dto))
                    {
                        dto.ActiveDriversCount++;
                        if (driver.Status == CurrentStatus.Available.ToString())
                        {
                            dto.AvailableDriversCount++;
                        }
                        else if (driver.Status == onTripStatusStr)
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
