using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneInsights
{
    public record GetZoneInsightsQuery(int ZoneId) : IRequest<Result<ZoneInsightsDto>>;
}
