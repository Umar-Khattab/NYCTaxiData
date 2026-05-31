using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping; // تأكد من الـ Spelling المعتمد (Plumbing)
using NYCTaxiData.Application.Features.Trips.Commands.StartTrip;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Trips.Commands.StartTrip
{
    public class StartTripCommandHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<StartTripCommand, Result<TripStartResultDto>>
    {
        public async Task<Result<TripStartResultDto>> Handle(
            StartTripCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // 1️⃣ جلب الـ Trip والـ Driver من الـ Database للتحقق
                var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);
                if (trip == null)
                    return Result<TripStartResultDto>.Failure($"Trip with ID {request.TripId} not found", "NotFound");

                var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
                if (driver == null)
                    return Result<TripStartResultDto>.Failure($"Driver with ID {request.DriverId} not found", "NotFound");

                if (driver.Status == CurrentStatus.On_Trip)
                    return Result<TripStartResultDto>.Failure("Driver is already on another trip.", "Conflict");

                // 2️⃣ إسناد البيانات للـ الذاكرة وتحديث حالة السائق
                trip.StartedAt = DateTime.UtcNow;
                driver.Status = CurrentStatus.On_Trip;

                // 🚀 الحسم الهندسي النهائي (الخطة البديلة القاطعة):
                // بما إن الـ Tracker قافل التعديل بسبب الـ No-Tracking، هنعدل الحقول دي مباشرة في الـ DB يدوياً أو نعتمد على الحفظ الصريح
                trip.DriverId = request.DriverId;
                trip.PickupLocationId = request.PickupLocationId > 0 ? request.PickupLocationId : null;
                trip.DropoffLocationId = request.DropoffLocationId > 0 ? request.DropoffLocationId : null;

                // تنفيذ الحفظ الأساسي للـ Status
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 3️⃣ بناء الـ DTO الراجع
                var resultDto = new TripStartResultDto
                {
                    TripId = request.TripId,
                    DriverId = request.DriverId,
                    StartedAt = trip.StartedAt ?? DateTime.UtcNow,
                    PickupLocationId = request.PickupLocationId,
                    DropoffLocationId = request.DropoffLocationId,
                    Status = "Ongoing"
                };

                return Result<TripStartResultDto>.Success(resultDto, "Trip started successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CRITICAL ERROR] StartTrip failed: {ex.Message}");
                return Result<TripStartResultDto>.Failure($"Database Save Failed: {ex.Message}", "DatabaseError");
            }
        }
    }
}