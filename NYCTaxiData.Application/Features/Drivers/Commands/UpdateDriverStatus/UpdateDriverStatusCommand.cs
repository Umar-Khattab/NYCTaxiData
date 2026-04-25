using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Drivers.Commands.UpdateDriverStatus
{
    public sealed record UpdateDriverStatusCommand(
        Guid DriverId,
        string Status,
        double CurrentLat,
        double CurrentLng)
        : IRequest<Result<Unit>>, ITransactionalCommand;
}
