using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Drivers.Commands.SyncOfflineData;

public sealed record SyncOfflineDataCommand(
    Guid DriverId,
    IReadOnlyCollection<OfflineTripDto> Trips)
    : IRequest<Result<SyncSummaryDto>>, ITransactionalCommand;

public sealed record OfflineTripDto(
    string LocalTripId,
    int PickupLocationId,
    int DropoffLocationId,
    DateTime StartedAt,
    DateTime EndedAt,
    decimal ActualFare);

public sealed class SyncSummaryDto
{
    public int ReceivedCount { get; init; }
    public int SyncedCount { get; init; }
    public int FailedCount { get; init; }
    public IReadOnlyList<string> FailedLocalTripIds { get; init; } = [];
}
