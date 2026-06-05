using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneMetadata
{
    public class GetZoneMetadataQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetZoneMetadataQuery, Result<ZoneMetadataDto>>
    {
        public async Task<Result<ZoneMetadataDto>> Handle(
            GetZoneMetadataQuery request,
            CancellationToken cancellationToken)
        {
            var zonesData = await _unitOfWork.Zones.Query()
                .AsNoTracking()
                .Select(z => new { z.OsmId, z.CenterLat, z.CenterLong })
                .ToListAsync(cancellationToken);

            var totalZones = zonesData.Count;

            var zonesWithOsmId = zonesData
                .Where(z => z.OsmId != null)
                .Select(z => z.OsmId)
                .Distinct()
                .Count();

            var zoneCounts = zonesData
                .Where(z => z.CenterLat != null && z.CenterLong != null && z.OsmId != null)
                .GroupBy(z => z.OsmId!.Value)
                .ToDictionary(g => g.Key.ToString(), g => g.Count());

            var metadata = new ZoneMetadataDto
            {
                TotalZones = totalZones,
                ZonesWithOsmId = zonesWithOsmId,
                OsmIdCounts = zoneCounts
            };

            return Result<ZoneMetadataDto>.Success(metadata, "Zone metadata retrieved successfully");
        }
    }
}
