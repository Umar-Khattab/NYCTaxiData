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
                .Where(z => z.CenterLat != null && z.CenterLong != null) 
                .Distinct()
                .CountAsync(cancellationToken);

            var totalServiceZones = await _unitOfWork.Zones.Query()
                .Where(z => z.OsmId != null  )
                .Select(z => z.OsmId)
                .Distinct()
                .CountAsync(cancellationToken);

            var zoneCountsList = await _unitOfWork.Zones.Query()
            .Where(z => z.CenterLat != null && z.CenterLong != null) // التأكد من وجود الإحداثيات
            .GroupBy(z => z.OsmId) // تجميع المناطق بناءً على الـ OsmId
            .Select(g => new {
                OsmId = g.Key, // المفتاح هو الـ OsmId
                Count = g.Count() // عدد المناطق المرتبطة بهذا الـ OsmId
            })
           .ToListAsync(cancellationToken);
             
            var zoneCounts = zoneCountsList.ToDictionary(x => x.OsmId.ToString(), x => x.Count);

            var metadata = new ZoneMetadataDto
            {
                TotalZones = totalZones,
                TotalBoroughs = totalBoroughs,
                TotalServiceZones = totalServiceZones,
                BoroughCounts = zoneCounts
            };

            return Result<ZoneMetadataDto>.Success(metadata, "Zone metadata retrieved successfully");
        }
    }
}
