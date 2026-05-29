using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetAllZones
{
    public record GetAllZonesQuery() : IRequest<Result<List<ZoneDto>>>;
}
