using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;
using System;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetRevenueStatistics
{
    public record GetRevenueStatisticsQuery(
        DateTime? StartDate = null,
        DateTime? EndDate = null
    ) : IRequest<Result<RevenueStatisticsDto>>;
}
