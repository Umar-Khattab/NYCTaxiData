using MediatR;
using NYCTaxiData.Application.Common.Plumbing; // تأكد من استخدام الـ Result الصح في مشروعك
using NYCTaxiData.Domain.Entities;
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
        var driverExists = await _unitOfWork.Drivers.AnyAsync(d => d.UserId == request.DriverId);
        if (!driverExists)
            return Result<SyncSummaryDto>.Failure($"Driver with id '{request.DriverId}' was not found.");

        var syncedCount = 0;
        var failedCount = 0; 
        var failedIds = new List<string>();

        foreach (var t in request.Trips)
        {
            try
            {
                // 1. تشييك ذكي: هل الـ Location ده موجود أصلاً؟
                bool pickupValid = await _unitOfWork.Locations.AnyAsync(l => l.LocationId == t.PickupLocationId);
                bool dropoffValid = await _unitOfWork.Locations.AnyAsync(l => l.LocationId == t.DropoffLocationId);

                // 2. لو أي واحد مش موجود، خلي الـ ID بتاعه null (لو الـ DB بتسمح) 
                // أو اعتبر الرحلة دي فاشلة وسجلها في الـ FailedList
                if (!pickupValid || !dropoffValid)
                {
                    throw new Exception("Invalid LocationId (Foreign Key Violation)");
                }

                var trip = new Trip
                {
                    DriverId = request.DriverId,
                    PickupLocationId = t.PickupLocationId,
                    DropoffLocationId = t.DropoffLocationId,
                    StartedAt = t.StartedAt,
                    EndedAt = t.EndedAt 
                };

                await _unitOfWork.Trips.AddAsync(trip);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                syncedCount++;
            }
            catch (Exception)
            {
                failedCount++; 
                failedIds.Add(t.LocalTripId.ToString());
            }
        }

        return Result<SyncSummaryDto>.Success(new SyncSummaryDto
        {
            ReceivedCount = request.Trips.Count,
            SyncedCount = syncedCount,
            FailedCount = failedCount, 
            FailedLocalTripIds = failedIds.Select(id => id.ToString()).ToList()
        }, "Sync process completed.");
    }
}