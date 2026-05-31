using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripById
{
    public record GetTripByIdQuery(int TripId) : IRequest<Result<TripDto>>;
}
