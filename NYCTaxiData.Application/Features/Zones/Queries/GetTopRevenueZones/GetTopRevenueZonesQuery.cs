using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetTopRevenueZones
{
    public record GetTopRevenueZonesQuery(int Limit = 10) : IRequest<Result<List<TopRevenueZoneDto>>>;
}
