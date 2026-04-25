using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Drivers.Commands.SyncOfflineData;

public sealed class SyncOfflineDataCommandHandler : IRequestHandler<SyncOfflineDataCommand, Result<SyncSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public SyncOfflineDataCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SyncSummaryDto>> Handle(SyncOfflineDataCommand request, CancellationToken cancellationToken)
    {
        var driverExists = await _unitOfWork.Drivers.AnyAsync(d => d.Id == request.DriverId);
        if (!driverExists)
        {
            return Result<SyncSummaryDto>.Failure($"Driver with id '{request.DriverId}' was not found.");
        }

        if (request.Trips.Count == 0)
        {
            return Result<SyncSummaryDto>.Success(new SyncSummaryDto
            {
                ReceivedCount = 0,
                SyncedCount = 0,
                FailedCount = 0,
                FailedLocalTripIds = []
            });
        }

        var tripsToPersist = request.Trips.Select(t => new Trip
        {
            DriverId = request.DriverId,
            PickupLocationId = t.PickupLocationId,
            DropoffLocationId = t.DropoffLocationId,
            StartedAt = t.StartedAt,
            EndedAt = t.EndedAt,
            ActualFare = t.ActualFare
        }).ToList();

        await _unitOfWork.Trips.AddRangeAsync(tripsToPersist);

        return Result<SyncSummaryDto>.Success(new SyncSummaryDto
        {
            ReceivedCount = request.Trips.Count,
            SyncedCount = request.Trips.Count,
            FailedCount = 0,
            FailedLocalTripIds = []
        });
    }
}
