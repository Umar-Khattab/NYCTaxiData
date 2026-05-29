using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneMetadata
{
    public record GetZoneMetadataQuery() : IRequest<Result<ZoneMetadataDto>>;
}
