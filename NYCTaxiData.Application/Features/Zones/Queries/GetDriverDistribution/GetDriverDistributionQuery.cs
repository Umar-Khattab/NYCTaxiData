using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using System.Collections.Generic;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetDriverDistribution
{
    public record GetDriverDistributionQuery() : IRequest<Result<List<DriverDistributionDto>>>;
}
