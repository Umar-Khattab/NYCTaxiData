using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using System;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetDemandStatistics
{
    public record GetDemandStatisticsQuery(
        DateTime? StartDate = null,
        DateTime? EndDate = null
    ) : IRequest<Result<DemandStatisticsDto>>;
}
