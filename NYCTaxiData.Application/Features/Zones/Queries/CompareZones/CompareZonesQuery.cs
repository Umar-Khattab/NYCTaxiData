using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Zones.Queries.CompareZones
{
    public record CompareZonesQuery(List<int> ZoneIds) : IRequest<Result<ZoneComparisonDto>>;
}
