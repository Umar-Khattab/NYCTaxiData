using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Tracking;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Data.Contexts; // عشان الـ DbContext

namespace NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch
{
    public class ManualDispatchCommandHandler(
        Domain.Interfaces.IUnitOfWork _unitOfWork,
        IMapper _mapper,
        IDispatchNotificationService _dispatchService,
        TaxiDbContext _context) // ضفنا الـ Context عشان الـ Concurrency Tracking
        : IRequestHandler<ManualDispatchCommand, Result<DispatchResultDto>>
    {
        public async Task<Result<DispatchResultDto>> Handle(
            ManualDispatchCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ جلب السائق مع Tracking عشان الـ RowVersion يشتغل صح
            var driver = await _context.Drivers
                .Include(d => d.IdNavigation) // عشان نجيب التليفون للـ Notification
                .FirstOrDefaultAsync(d => d.Id == request.DriverId, cancellationToken);

            if (driver == null)
                return Result<DispatchResultDto>.Failure($"Driver with ID {request.DriverId} not found", "NotFound");

            // ✅ التآكد من الحالة (فقط السائق المتاح يمكن تعيينه)
            if (driver.Status != CurrentStatus.Available)
                return Result<DispatchResultDto>.Failure($"Driver is {driver.Status}. Only Available drivers can be dispatched.", "Conflict");

            // 2️⃣ التحقق من وجود المناطق (Pickup & Dropoff)
            var pickupZone = await _unitOfWork.Zones.GetByIdAsync(request.PickupZoneId);
            if (pickupZone == null)
                return Result<DispatchResultDto>.Failure($"Pickup zone {request.PickupZoneId} not found", "NotFound");

            var dropoffZone = await _unitOfWork.Zones.GetByIdAsync(request.DropoffZoneId);
            if (dropoffZone == null)
                return Result<DispatchResultDto>.Failure($"Dropoff zone {request.DropoffZoneId} not found", "NotFound");

            // 3️⃣ جلب المواقع داخل المناطق
            var pickupLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == request.PickupZoneId);
            if (pickupLocations == null || !pickupLocations.Any())
                return Result<DispatchResultDto>.Failure("No locations found in pickup zone", "NotFound");

            var dropoffLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == request.DropoffZoneId);
            if (dropoffLocations == null || !dropoffLocations.Any())
                return Result<DispatchResultDto>.Failure("No locations found in dropoff zone", "NotFound");

            // 4️⃣ إنشاء الرحلة وتغيير حالة السائق
            var trip = new Trip
            {
                DriverId = request.DriverId,
                PickupLocationId = pickupLocations.First().LocationId,
                DropoffLocationId = dropoffLocations.First().LocationId,
                StartedAt = null
            };

            driver.Status = CurrentStatus.On_Trip;

            try
            {
                await _unitOfWork.Trips.AddAsync(trip);
                // ✅ الحفظ مع مراقبة الـ Concurrency
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // ✅ لو في حد عدل السائق في نفس اللحظة
                return Result<DispatchResultDto>.Failure("Driver status has changed. Please try again.", "Conflict");
            }

            // 5️⃣ إرسال التنبيه الفوري (SignalR)
            var driverPhone = driver.IdNavigation?.Phonenumber ?? "01111128427";

            await _dispatchService.SendDispatchToDriverAsync(driverPhone, new DispatchNotificationDto
            {
                DriverPhone = driverPhone,
                TargetZoneId = request.PickupZoneId.ToString(),
                TargetZoneName = pickupZone.ZoneName ?? "Unknown",
                Priority = "High",
                Message = $"New Trip Assigned: From {pickupZone.ZoneName} to {dropoffZone.ZoneName}",
                IssuedAt = DateTime.UtcNow
            }, cancellationToken);

            // 6️⃣ تحضير الرد النهائي
            var resultDto = _mapper.Map<DispatchResultDto>(trip);
            resultDto.PickupZoneId = request.PickupZoneId;
            resultDto.DropoffZoneId = request.DropoffZoneId;
            resultDto.PassengerName = request.PassengerName;
            resultDto.DispatchId = $"DSP-{trip.TripId}";

            return Result<DispatchResultDto>.Success(resultDto, "Manual dispatch completed successfully");
        }
    }
}