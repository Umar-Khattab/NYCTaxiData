using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Trips.Commands.DeleteTrip
{
    public record DeleteTripCommand(int TripId) : IRequest<Result<bool>>, ITransactionalCommand, ISecureRequest;
}
