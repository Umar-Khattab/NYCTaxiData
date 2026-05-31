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

            var totalBoroughs = await _unitOfWork.Zones.Query()
                .Where(z => z.Borough != null && z.Borough != "")
                .Select(z => z.Borough)
                .Distinct()
                .CountAsync(cancellationToken);

            var totalServiceZones = await _unitOfWork.Zones.Query()
                .Where(z => z.ServiceZone != null && z.ServiceZone != "")
                .Select(z => z.ServiceZone)
                .Distinct()
                .CountAsync(cancellationToken);

            var boroughCountsList = await _unitOfWork.Zones.Query()
                .Where(z => z.Borough != null && z.Borough != "")
                .GroupBy(z => z.Borough)
                .Select(g => new { Borough = g.Key!, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var boroughCounts = boroughCountsList.ToDictionary(x => x.Borough, x => x.Count);

            var metadata = new ZoneMetadataDto
            {
                TotalZones = totalZones,
                TotalBoroughs = totalBoroughs,
                TotalServiceZones = totalServiceZones,
                BoroughCounts = boroughCounts
            };

            return Result<ZoneMetadataDto>.Success(metadata, "Zone metadata retrieved successfully");
        }
    }
}
