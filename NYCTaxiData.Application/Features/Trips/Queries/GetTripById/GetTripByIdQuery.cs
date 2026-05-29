using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripById
{
    public record GetTripByIdQuery(int TripId) : IRequest<Result<TripDto>>;
}
