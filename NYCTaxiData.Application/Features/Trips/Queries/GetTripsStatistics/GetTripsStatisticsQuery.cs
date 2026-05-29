using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripsStatistics
{
    public record GetTripsStatisticsQuery() : IRequest<Result<TripStatisticsDto>>;
}
