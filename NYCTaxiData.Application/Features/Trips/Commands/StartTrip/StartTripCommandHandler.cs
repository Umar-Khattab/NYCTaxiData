using MediatR;
using AutoMapper;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Domain.Enums;
using Microsoft.EntityFrameworkCore;


namespace NYCTaxiData.Application.Features.Trips.Commands.StartTrip
{
    public class StartTripCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<StartTripCommand, Result<TripStartResultDto>>
    {
        public async Task<Result<TripStartResultDto>> Handle(
            StartTripCommand request,
            CancellationToken cancellationToken)
        {
            // 1. جلب السائق والتحقق من حالته (Optimistic Concurrency)
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
            if (driver == null)
                throw new NotFoundException("Driver", request.DriverId);

            // Business Validation: هل السائق مشغول فعلاً؟
            if (driver.Status == CurrentStatus.On_Trip)
                return Result<TripStartResultDto>.Failure("Driver is already on another trip", "Conflict");

            // 2. التحقق من وجود المواقع (كود صاحبك)
            var pickupLocation = await _unitOfWork.Locations.GetByIdAsync(request.PickupLocationId);
            if (pickupLocation == null)
                throw new NotFoundException("Pickup Location", request.PickupLocationId);

            var dropoffLocation = await _unitOfWork.Locations.GetByIdAsync(request.DropoffLocationId);
            if (dropoffLocation == null)
                throw new NotFoundException("Dropoff Location", request.DropoffLocationId);

            // 3. إنشاء الرحلة وتحديث حالة السائق
            var trip = new Trip
            {
                DriverId = request.DriverId,
                PickupLocationId = request.PickupLocationId,
                DropoffLocationId = request.DropoffLocationId,
                StartedAt = DateTime.UtcNow
            };

            driver.Status = CurrentStatus.On_Trip;

            try
            {
                // إضافة الرحلة وحفظ التغييرات (اللي هتشمل تحديث السائق والرحلة معاً)
                await _unitOfWork.Trips.AddAsync(trip);

                // الـ SaveChanges هنا هي اللي هتفحص الـ RowVersion بتاع الـ Driver
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // لو سواق تاني خده في نفس اللحظة، الـ RowVersion هيتغير والـ Exception ده هيضرب
                var entry = ex.Entries.First();
                var entityName = entry.Entity.GetType().Name;

                // تحديث البيانات المحلية عشان نعرف مين اللي عدل
                await entry.ReloadAsync(cancellationToken);

                return Result<TripStartResultDto>.Failure(
                    $"{entityName} was modified by another process. Please try again.",
                    "ConcurrencyConflict");
            }

            // 4. تحويل النتيجة وإرجاعها
            var resultDto = _mapper.Map<TripStartResultDto>(trip);
            return Result<TripStartResultDto>.Success(resultDto, "Trip started successfully");
        }
    }
}