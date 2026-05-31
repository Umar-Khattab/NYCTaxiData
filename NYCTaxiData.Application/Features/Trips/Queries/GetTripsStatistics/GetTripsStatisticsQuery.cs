using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripsStatistics
{
    public record GetTripsStatisticsQuery() : IRequest<Result<TripStatisticsDto>>;
}
