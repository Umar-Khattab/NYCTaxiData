using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Commands.DeleteTrip;

public sealed record DeleteTripCommand(int TripId)
    : IRequest<Result<TripDeleteResultDto>>, ITransactionalCommand, ISecureRequest;
