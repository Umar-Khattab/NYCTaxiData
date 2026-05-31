using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.DTOs.Trip;
using System;

namespace NYCTaxiData.Application.Features.Trips.Commands.CreateTrip
{
    public record CreateTripCommand(
        Guid DriverId,
        int PickupLocationId,
        int DropoffLocationId,
        decimal FareAmount,
        decimal TipAmount
    ) : IRequest<Result<TripDto>>, ITransactionalCommand, ISecureRequest;
}
