using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneInsights
{
    public class GetZoneInsightsQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetZoneInsightsQuery, Result<ZoneInsightsDto>>
    {
        public async Task<Result<ZoneInsightsDto>> Handle(
            GetZoneInsightsQuery request,
            CancellationToken cancellationToken)
        {
            var zone = await _unitOfWork.Zones.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.ZoneId == request.ZoneId, cancellationToken);

            if (zone == null)
                return Result<ZoneInsightsDto>.Failure($"Zone with ID {request.ZoneId} not found", "NotFound");

            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
            var zoneDict = zones.ToDictionary(z => z.ZoneId, z => z);

            // 2. Identify Peak Period based on busiest hour
            var busiestHourGroup = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId == request.ZoneId && t.StartedAt != null)
                .GroupBy(t => t.StartedAt!.Value.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync(cancellationToken);

            string peakPeriodName = "Afternoon Slack";
            if (busiestHourGroup != null)
            {
                int h = busiestHourGroup.Hour;
                if (h >= 7 && h <= 10) peakPeriodName = "Morning Rush";
                else if (h >= 11 && h <= 15) peakPeriodName = "Midday Core";
                else if (h >= 16 && h <= 19) peakPeriodName = "Evening Rush";
                else if (h >= 20 || h <= 4) peakPeriodName = "Night Owl";
                else peakPeriodName = "Early Bird";
            }

            // 3. Simulated insights
            double avgWaitTime = 4.5 + (request.ZoneId % 5) * 1.5;
            decimal driverEff = 85.0m + (request.ZoneId % 10) * 1.2m;

            var insights = new ZoneInsightsDto
            {
                ZoneId = request.ZoneId,
                ZoneName = zone.ZoneName,
                AvgWaitTimeMinutes = avgWaitTime,
                PeakPeriodName = peakPeriodName,
                DriverEfficiencyScore = driverEff
            };

            return Result<ZoneInsightsDto>.Success(insights, "Zone insights retrieved successfully");
        }
    }
}
