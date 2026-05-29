using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripById;

public sealed record GetTripByIdQuery(int TripId)
    : IRequest<Result<TripDto>>, ICacheableQuery;
