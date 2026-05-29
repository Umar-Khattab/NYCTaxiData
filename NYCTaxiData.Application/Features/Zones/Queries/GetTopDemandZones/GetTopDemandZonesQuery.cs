using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetTopDemandZones
{
    public record GetTopDemandZonesQuery(int Limit = 10) : IRequest<Result<List<TopDemandZoneDto>>>;
}
