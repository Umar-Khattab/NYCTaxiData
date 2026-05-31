using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Plumbing; // تأكد من الـ Spelling المعتمد (Plumbing)
using NYCTaxiData.Application.DTOs.Tracking;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;

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
            try
            {
                // 1️⃣ التحقق من بناء الـ TripId وحمايته من التكرار (Unique Check)
                int finalTripId = request.TripId is int id ? id : (request.TripId ?? 0);

                if (finalTripId > 0)
                {
                    var existingTrip = await _unitOfWork.Trips.GetByIdAsync(finalTripId);
                    if (existingTrip != null)
                    {
                        return Result<DispatchResultDto>.Failure(
                            $"Trip with ID {finalTripId} already exists. Please use a unique Trip ID.",
                            "DuplicateTripId");
                    }
                }

                // 2️⃣ التحقق من وجود السائق
                var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
                if (driver == null)
                    return Result<DispatchResultDto>.Failure($"Driver with ID {request.DriverId} not found", "DriverNotFound");

                int pZoneId = request.PickupZoneId;
                int dZoneId = request.DropoffZoneId;

                // 3️⃣ جلب المناطق والمواقع بأمان بدون كراش
                var pickupZone = await _unitOfWork.Zones.GetByIdAsync(pZoneId);
                var dropoffZone = await _unitOfWork.Zones.GetByIdAsync(dZoneId);

                string pickupZoneName = pickupZone?.ZoneName ?? $"Zone {pZoneId}";
                string dropoffZoneName = dropoffZone?.ZoneName ?? $"Zone {dZoneId}";

                var pickupLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == pZoneId);
                var dropoffLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == dZoneId);

                // 🚀 الـتـقـفـيـل الـسـحـري: لو الجداول فاضية باصي null صريحة عشان الـ DB توافق علطول وتهرب من الـ Constraints
                int? finalPickupLocationId = (pickupLocations != null && pickupLocations.Any())
                    ? pickupLocations.First().LocationId
                    : null;

                int? finalDropoffLocationId = (dropoffLocations != null && dropoffLocations.Any())
                    ? dropoffLocations.First().LocationId
                    : null;

                // 4️⃣ بناء الـ Trip Entity
                var trip = new Trip
                {
                    TripId = finalTripId,
                    DriverId = request.DriverId,
                    PickupLocationId = finalPickupLocationId, // هتنزل null بأمان في الـ DB
                    DropoffLocationId = finalDropoffLocationId, // هتنزل null بأمان في الـ DB
                    CreatedAt = DateTime.UtcNow,
                    StartedAt = DateTime.UtcNow,
                    EndedAt = null
                };

                // 5️⃣ تحديث حالة السائق لـ مشغول برحلة
                driver.Status = CurrentStatus.On_Trip;

                // حفظ التغييرات في قاعدة البيانات (Supabase)
                await _unitOfWork.Trips.AddAsync(trip);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 6️⃣ إرسال الإشعار للسائق (محمية ضد الكراش)
                var driverPhone = driver.User?.PhoneNumber ?? request.PassengerPhone;

                try
                {
                    await _dispatchService.SendDispatchToDriverAsync(driverPhone, new DispatchNotificationDto
                    {
                        DriverPhone = driverPhone,
                        TargetZoneId = pZoneId.ToString(),
                        TargetZoneName = pickupZoneName,
                        Priority = request.Priority ?? "CRITICAL",
                        Message = $"New Trip Assigned: From {pickupZoneName} to {dropoffZoneName}",
                        IssuedAt = DateTime.UtcNow
                    }, cancellationToken);
                }
                catch (Exception notifyEx)
                {
                    Console.WriteLine($"⚠️ Notification Service Postponed: {notifyEx.Message}");
                }

                // 7️⃣ الـ Manual Mapping النظيف المتوافق مع الـ DTO
                var resultDto = new DispatchResultDto
                {
                    DriverId = request.DriverId,
                    PickupZoneId = pZoneId,
                    DropoffZoneId = dZoneId,
                    PassengerName = request.PassengerName,
                    Status = "Dispatched Successfully"
                };

                return Result<DispatchResultDto>.Success(resultDto, "Manual dispatch processed successfully");
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.Message != null && ex.InnerException.Message.Contains("23505"))
                {
                    return Result<DispatchResultDto>.Failure(
                        "This Trip ID has just been taken by another transaction. Please try again with a new ID.",
                        "DuplicateTripId");
                }

                Console.WriteLine($"❌ [FATAL ERROR] ManualDispatch Handler crashed: {ex.Message}");
                return Result<DispatchResultDto>.Failure(
                    $"An unexpected system error occurred: {ex.Message}",
                    "InternalServerError");
            }
        }
    }
}