using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Plumping; // تأكد من المسار الصحيح للـ Result
using NYCTaxiData.Application.DTOs.Tracking;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch
{
    public class ManualDispatchCommandHandler(
        Domain.Interfaces.IUnitOfWork _unitOfWork,
        IMapper _mapper,
        IDispatchNotificationService _dispatchService)
        : IRequestHandler<ManualDispatchCommand, Result<DispatchResultDto>>
    {
        public async Task<Result<DispatchResultDto>> Handle(
            ManualDispatchCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ التحقق من وجود السائق وحالته
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
            if (driver == null)
                return Result<DispatchResultDto>.Failure($"Driver with ID {request.DriverId} not found");

            // 2️⃣ التحقق من وجود المناطق (Pickup & Dropoff)
            var pickupZone = await _unitOfWork.Zones.GetByIdAsync(request.PickupZoneId);
            if (pickupZone == null)
                return Result<DispatchResultDto>.Failure($"Pickup zone with ID {request.PickupZoneId} not found");

            var dropoffZone = await _unitOfWork.Zones.GetByIdAsync(request.DropoffZoneId);
            if (dropoffZone == null)
                return Result<DispatchResultDto>.Failure($"Dropoff zone with ID {request.DropoffZoneId} not found");

            // 3️⃣ التحقق من وجود مواقع داخل المناطق
            var pickupLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == request.PickupZoneId);
            if (pickupLocations == null || !pickupLocations.Any())
                return Result<DispatchResultDto>.Failure($"No locations found in pickup zone {request.PickupZoneId}");

            var dropoffLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == request.DropoffZoneId);
            if (dropoffLocations == null || !dropoffLocations.Any())
                return Result<DispatchResultDto>.Failure($"No locations found in dropoff zone {request.DropoffZoneId}");

            // 4️⃣ إنشاء الرحلة الجديدة وتحديث حالة السائق
            var trip = new Trip
            {
                DriverId = request.DriverId,
                PickupLocationId = pickupLocations.First().LocationId,
                DropoffLocationId = dropoffLocations.First().LocationId,
                StartedAt = null,
                EndedAt = null
            };

            // تحديث حالة السائق إلى "On_Trip"
            driver.Status = CurrentStatus.On_Trip;

            await _unitOfWork.Trips.AddAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            // 5️⃣ إرسال التنبيه الفوري عبر SignalR
            var driverPhone = driver.IdNavigation?.Phonenumber ?? "01111128427"; // Fallback للتجربة

            await _dispatchService.SendDispatchToDriverAsync(driverPhone, new DispatchNotificationDto
            {
                DriverPhone = driverPhone,
                TargetZoneId = request.PickupZoneId.ToString(),
                TargetZoneName = pickupZone.ZoneName,
                Priority = "High",
                Message = $"New Trip Assigned: From {pickupZone.ZoneName} to {dropoffZone.ZoneName}",
                IssuedAt = DateTime.UtcNow
            }, cancellationToken);

            // 6️⃣ تحضير الرد النهائي
            var resultDto = _mapper.Map<DispatchResultDto>(trip);
            resultDto.PickupZoneId = request.PickupZoneId;
            resultDto.DropoffZoneId = request.DropoffZoneId;
            resultDto.PassengerName = request.PassengerName;

            return Result<DispatchResultDto>.Success(resultDto);
        }
    }
}