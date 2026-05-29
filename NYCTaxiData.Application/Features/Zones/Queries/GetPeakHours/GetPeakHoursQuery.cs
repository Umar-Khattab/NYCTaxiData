using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetPeakHours
{
    public record GetPeakHoursQuery(int? ZoneId) : IRequest<Result<List<PeakHoursDto>>>;
}
