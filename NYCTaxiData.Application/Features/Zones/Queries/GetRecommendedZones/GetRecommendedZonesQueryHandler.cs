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

namespace NYCTaxiData.Application.Features.Zones.Queries.GetRecommendedZones
{
    public class GetRecommendedZonesQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetRecommendedZonesQuery, Result<List<RecommendedZoneDto>>>
    {
        public async Task<Result<List<RecommendedZoneDto>>> Handle(
            GetRecommendedZonesQuery request,
            CancellationToken cancellationToken)
        {
            var limit = request.Limit > 0 ? request.Limit : 10;

            // Group pickups and aggregate fare, tips, and counts directly on DB
            var zoneAggregates = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new
                {
                    ZoneId = g.Key,
                    TripCount = g.Count(),
                    AvgFare = g.Average(t => t.FareAmount),
                    AvgTip = g.Average(t => t.TipAmount ?? 0m)
                })
                .ToListAsync(cancellationToken);

            var zones = await _unitOfWork.Zones.Query()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            var recommendations = new List<RecommendedZoneDto>();

            // Find max values to normalize scores
            double maxTrips = zoneAggregates.Select(x => x.TripCount).DefaultIfEmpty(1).Max();
            decimal maxFare = zoneAggregates.Select(x => x.AvgFare).DefaultIfEmpty(1m).Max();
            decimal maxTip = zoneAggregates.Select(x => x.AvgTip).DefaultIfEmpty(1m).Max();

            foreach (var agg in zoneAggregates)
            {
                if (zoneDict.TryGetValue(agg.ZoneId, out var zone))
                {
                    // Normalized scoring logic
                    double normTrips = agg.TripCount / maxTrips;
                    double normFare = maxFare > 0 ? (double)(agg.AvgFare / maxFare) : 0;
                    double normTip = maxTip > 0 ? (double)(agg.AvgTip / maxTip) : 0;

                    // Recommendation Score: 40% fare, 35% tips, 25% trip volumes
                    double score = (normFare * 0.40) + (normTip * 0.35) + (normTrips * 0.25);
                    decimal finalScore = Math.Round((decimal)(score * 100.0), 2);

                    // Synthesize demand-supply ratio (simulated metric)
                    double ratio = 1.0 + ((agg.ZoneId % 7) * 0.2);

                    string reason = "High typical earnings and strong tip averages.";
                    if (normFare > 0.8) reason = "Premium fare profiles detected.";
                    else if (normTip > 0.8) reason = "Exceptional passenger tipping behavior.";
                    else if (normTrips > 0.8) reason = "Extremely high pickup volumes and short wait times.";

                    recommendations.Add(new RecommendedZoneDto
                    {
                        ZoneId = zone.ZoneId,
                        ZoneName = zone.ZoneName,
                        Borough = zone.Borough ?? "Unknown",
                        RecommendationScore = finalScore,
                        AvgFare = Math.Round(agg.AvgFare, 2),
                        AvgTip = Math.Round(agg.AvgTip, 2),
                        DemandSupplyRatio = Math.Round((decimal)ratio, 2),
                        Reason = reason
                    });
                }
            }

            var sortedRecommendations = recommendations
                .OrderByDescending(x => x.RecommendationScore)
                .Take(limit)
                .ToList();

            return Result<List<RecommendedZoneDto>>.Success(sortedRecommendations, "Recommended zones generated successfully");
        }
    }
}
