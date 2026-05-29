using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetHighStockoutZones
{
    public class GetHighStockoutZonesQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetHighStockoutZonesQuery, Result<List<HighStockoutZoneDto>>>
    {
        public async Task<Result<List<HighStockoutZoneDto>>> Handle(
            GetHighStockoutZonesQuery request,
            CancellationToken cancellationToken)
        {
            var limit = request.Limit > 0 ? request.Limit : 10;

            // 1. Demand: Get pickup counts per zone
            var demandList = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, PickupCount = g.Count() })
                .ToListAsync(cancellationToken);

            var demandDict = demandList.ToDictionary(d => d.ZoneId, d => d.PickupCount);

            // 2. Supply: Get driver distribution
            var activeDriversInfo = await _unitOfWork.Drivers.Query()
                .AsNoTracking()
                .Where(d => d.Status == CurrentStatus.Available)
                .Select(d => new
                {
                    DriverId = d.UserId,
                    LatestLocationId = d.Trips
                        .OrderByDescending(t => t.StartedAt)
                        .Select(t => t.DropoffLocationId)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var locations = await _unitOfWork.Locations.Query().AsNoTracking().ToListAsync(cancellationToken);
            var locDict = locations.ToDictionary(l => l.LocationId, l => l);
            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);

            var driverSupplyDict = zones.ToDictionary(z => z.ZoneId, z => 0);

            foreach (var driver in activeDriversInfo)
            {
                if (driver.LatestLocationId.HasValue && locDict.TryGetValue(driver.LatestLocationId.Value, out var loc) && loc.ZoneId.HasValue)
                {
                    driverSupplyDict[loc.ZoneId.Value]++;
                }
            }

            // 3. Compute Deficit & Stockout Probability
            var stockoutList = new List<HighStockoutZoneDto>();

            foreach (var zone in zones)
            {
                int pickups = demandDict.GetValueOrDefault(zone.ZoneId, 0);
                int availableDrivers = driverSupplyDict.GetValueOrDefault(zone.ZoneId, 0);

                int deficit = Math.Max(0, pickups - availableDrivers);
                double prob = 0.0;

                if (pickups > 0)
                {
                    prob = (double)pickups / (pickups + availableDrivers + 1);
                }

                stockoutList.Add(new HighStockoutZoneDto
                {
                    ZoneId = zone.ZoneId,
                    ZoneName = zone.ZoneName,
                    Borough = zone.Borough ?? "Unknown",
                    PickupCount = pickups,
                    AvailableDriversCount = availableDrivers,
                    DeficitCount = deficit,
                    StockoutProbability = Math.Round(prob, 4)
                });
            }

            var topStockouts = stockoutList
                .OrderByDescending(x => x.StockoutProbability)
                .ThenByDescending(x => x.DeficitCount)
                .Take(limit)
                .ToList();

            return Result<List<HighStockoutZoneDto>>.Success(topStockouts, "High stockout risk zones identified successfully");
        }
    }
}
