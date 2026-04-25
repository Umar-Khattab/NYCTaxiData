using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.Tracking;
using NYCTaxiData.Domain.Enums; 
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch
{
    public class ManualDispatchCommandHandler(
        Domain.Interfaces.IUnitOfWork _unitOfWork,
        IMapper _mapper,
        IDispatchNotificationService _dispatchService) // ✅ حقن خدمة التنبيهات
        : IRequestHandler<ManualDispatchCommand, DispatchResultDto>
    {
        public async Task<DispatchResultDto> Handle(
            ManualDispatchCommand request,
            CancellationToken cancellationToken)
        {
            // 1️⃣ التحقق من وجود السائق وحالته (بما في ذلك رقم التليفون للـ SignalR)
            // ملاحظة: تأكد أن Repository السائق يجلب بيانات الـ Navigation (User)
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);

            if (driver == null)
                throw new NotFoundException($"Driver with ID {request.DriverId} not found");

            // 2️⃣ التحقق من وجود المناطق (Pickup & Dropoff)
            var pickupZone = await _unitOfWork.Zones.GetByIdAsync(request.PickupZoneId);
            if (pickupZone == null)
                throw new NotFoundException($"Pickup zone with ID {request.PickupZoneId} not found");

            var dropoffZone = await _unitOfWork.Zones.GetByIdAsync(request.DropoffZoneId);
            if (dropoffZone == null)
                throw new NotFoundException($"Dropoff zone with ID {request.DropoffZoneId} not found");

            // 3️⃣ التحقق من وجود مواقع داخل المناطق
            var pickupLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == request.PickupZoneId);
            if (pickupLocations == null || !pickupLocations.Any())
                throw new NotFoundException($"No locations found in pickup zone {request.PickupZoneId}");

            var dropoffLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == request.DropoffZoneId);
            if (dropoffLocations == null || !dropoffLocations.Any())
                throw new NotFoundException($"No locations found in dropoff zone {request.DropoffZoneId}");

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
            // الـ Update للـ Driver غالباً بيحصل تلقائياً لو الـ Tracking شغال أو تنادي Update صريح
            // await _unitOfWork.Drivers.UpdateAsync(driver); 

            await _unitOfWork.SaveChangesAsync();

            // 5️⃣ الخطوة السحرية: إرسال الـ Push Notification للسائق فوراً عبر SignalR
            // بنسحب رقم التليفون من الـ Navigation property (User)
            var driverPhone = driver.IdNavigation?.Phonenumber;
            if (string.IsNullOrEmpty(driverPhone))
            {
                driverPhone = "01111128427";
                Console.WriteLine("[Warning] Driver Phone was null, using fallback phone for testing.");
            }

            if (!string.IsNullOrEmpty(driverPhone))
            {
                await _dispatchService.SendDispatchToDriverAsync(driverPhone, new DispatchNotificationDto
                {
                    DriverPhone = driverPhone,
                    TargetZoneId = request.PickupZoneId.ToString(), // المنطقة اللي هيروح يحمل منها
                    TargetZoneName = pickupZone.ZoneName,
                    Priority = "High", // أو مررها من الـ Request
                    Message = $"New Trip Assigned: From {pickupZone.ZoneName} to {dropoffZone.ZoneName}",
                    IssuedAt = DateTime.UtcNow
                }, cancellationToken);
            }

            // 6️⃣ تحضير الرد النهائي للـ API
            var result = _mapper.Map<DispatchResultDto>(trip);
            result.PickupZoneId = request.PickupZoneId;
            result.DropoffZoneId = request.DropoffZoneId;
            result.PassengerName = request.PassengerName;

            return result;
        }
    }
}