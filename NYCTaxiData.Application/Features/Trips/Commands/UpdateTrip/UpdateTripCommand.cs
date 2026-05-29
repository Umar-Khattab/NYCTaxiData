using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.DTOs.Trip;

namespace NYCTaxiData.Application.Features.Trips.Commands.UpdateTrip
{
    public record UpdateTripCommand(
        int TripId,
        decimal FareAmount,
        decimal TipAmount,
        string ProcessStatus
    ) : IRequest<Result<TripDto>>, ITransactionalCommand, ISecureRequest;
}
