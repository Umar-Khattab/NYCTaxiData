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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetHeatmapData
{
    public class GetHeatmapDataQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetHeatmapDataQuery, Result<List<HeatmapDataPointDto>>>
    {
        public async Task<Result<List<HeatmapDataPointDto>>> Handle(
            GetHeatmapDataQuery request,
            CancellationToken cancellationToken)
        {
            // Execute GroupBy pickup location on database
            var tripCountsByZone = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var zones = await _unitOfWork.Zones.Query()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var heatmapPoints = new List<HeatmapDataPointDto>();

            foreach (var item in tripCountsByZone)
            {
                if (zoneDict.TryGetValue(item.ZoneId, out var zone))
                {
                    // Synthesize NYC Centroid coordinates based on zoneId
                    // Centered around Midtown Manhattan
                    double baseLat = 40.7306;
                    double baseLon = -73.9352;
                    double latOffset = ((item.ZoneId * 17) % 100) * 0.001 - 0.05;
                    double lonOffset = ((item.ZoneId * 23) % 100) * 0.001 - 0.05;

                    decimal surgeMultiplier = 1.0m;
                    string demandLevel = "LOW";

                    if (item.Count > 500)
                    {
                        surgeMultiplier = 2.2m;
                        demandLevel = "CRITICAL";
                    }
                    else if (item.Count > 200)
                    {
                        surgeMultiplier = 1.7m;
                        demandLevel = "ELEVATED";
                    }
                    else if (item.Count > 50)
                    {
                        surgeMultiplier = 1.2m;
                        demandLevel = "NORMAL";
                    }

                    heatmapPoints.Add(new HeatmapDataPointDto
                    {
                        ZoneId = zone.ZoneId,
                        ZoneName = zone.ZoneName,
                        Borough = zone.Borough ?? "Unknown",
                        Latitude = Math.Round(baseLat + latOffset, 4),
                        Longitude = Math.Round(baseLon + lonOffset, 4),
                        TripCount = item.Count,
                        SurgeMultiplier = surgeMultiplier,
                        DemandLevel = demandLevel
                    });
                }
            }

            // Fill in zones that have 0 trips to prevent visual gaps
            foreach (var zone in zones)
            {
                if (heatmapPoints.All(hp => hp.ZoneId != zone.ZoneId))
                {
                    double baseLat = 40.7306;
                    double baseLon = -73.9352;
                    double latOffset = ((zone.ZoneId * 17) % 100) * 0.001 - 0.05;
                    double lonOffset = ((zone.ZoneId * 23) % 100) * 0.001 - 0.05;

                    heatmapPoints.Add(new HeatmapDataPointDto
                    {
                        ZoneId = zone.ZoneId,
                        ZoneName = zone.ZoneName,
                        Borough = zone.Borough ?? "Unknown",
                        Latitude = Math.Round(baseLat + latOffset, 4),
                        Longitude = Math.Round(baseLon + lonOffset, 4),
                        TripCount = 0,
                        SurgeMultiplier = 1.0m,
                        DemandLevel = "LOW"
                    });
                }
            }

            return Result<List<HeatmapDataPointDto>>.Success(heatmapPoints.OrderByDescending(x => x.TripCount).ToList(), "Heatmap data retrieved successfully");
        }
    }
}
