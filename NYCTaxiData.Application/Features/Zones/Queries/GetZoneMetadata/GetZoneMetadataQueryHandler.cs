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
            var totalZones = await _unitOfWork.Zones.Query().CountAsync(cancellationToken);

            var zonesWithOsmId = await _unitOfWork.Zones.Query()
                .Where(z => z.OsmId != null)
                .Select(z => z.OsmId)
                .Distinct()
                .CountAsync(cancellationToken);

            var zoneCountsList = await _unitOfWork.Zones.Query()
                .Where(z => z.CenterLat != null && z.CenterLong != null && z.OsmId != null)
                .GroupBy(z => z.OsmId)
                .Select(g => new {
                    OsmId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);
             
            var zoneCounts = zoneCountsList
                .Where(x => x.OsmId.HasValue)
                .ToDictionary(x => x.OsmId!.Value.ToString(), x => x.Count);

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
