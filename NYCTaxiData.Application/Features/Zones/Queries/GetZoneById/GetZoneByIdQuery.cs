using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneById
{
    public record GetZoneByIdQuery(int ZoneId) : IRequest<Result<ZoneDto>>;
}
