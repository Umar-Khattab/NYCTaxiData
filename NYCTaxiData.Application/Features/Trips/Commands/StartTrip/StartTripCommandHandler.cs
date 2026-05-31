using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumbing; //  √ﬂœ „‰ «·‹ Spelling «·„⁄ „œ (Plumbing)
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
                // 1?? Ã·» «·‹ Trip Ê«·‹ Driver „‰ «·‹ Database ·· Õﬁﬁ
                var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);
                if (trip == null)
                    return Result<TripStartResultDto>.Failure($"Trip with ID {request.TripId} not found", "NotFound");

                var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
                if (driver == null)
                    return Result<TripStartResultDto>.Failure($"Driver with ID {request.DriverId} not found", "NotFound");

                if (driver.Status == CurrentStatus.On_Trip)
                    return Result<TripStartResultDto>.Failure("Driver is already on another trip.", "Conflict");

                // 2?? ≈”‰«œ «·»Ì«‰«  ··‹ «·–«ﬂ—… Ê ÕœÌÀ Õ«·… «·”«∆ﬁ
                trip.StartedAt = DateTime.UtcNow;
                driver.Status = CurrentStatus.On_Trip;

                // ?? «·Õ”„ «·Â‰œ”Ì «·‰Â«∆Ì («·Œÿ… «·»œÌ·… «·ﬁ«ÿ⁄…):
                // »„« ≈‰ «·‹ Tracker ﬁ«›· «· ⁄œÌ· »”»» «·‹ No-Tracking° Â‰⁄œ· «·ÕﬁÊ· œÌ „»«‘—… ›Ì «·‹ DB ÌœÊÌ« √Ê ‰⁄ „œ ⁄·Ï «·Õ›Ÿ «·’—ÌÕ
                trip.DriverId = request.DriverId;
                trip.PickupLocationId = request.PickupLocationId > 0 ? request.PickupLocationId : null;
                trip.DropoffLocationId = request.DropoffLocationId > 0 ? request.DropoffLocationId : null;

                //  ‰›Ì– «·Õ›Ÿ «·√”«”Ì ··‹ Status
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // 3?? »‰«¡ «·‹ DTO «·—«Ã⁄
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
                Console.WriteLine($"? [CRITICAL ERROR] StartTrip failed: {ex.Message}");
                return Result<TripStartResultDto>.Failure($"Database Save Failed: {ex.Message}", "DatabaseError");
            }
        }
    }
}