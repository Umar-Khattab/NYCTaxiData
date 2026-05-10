using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Trips.Commands.StartTrip
{
    public class StartTripCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<StartTripCommand, Result<TripStartResultDto>>
    {
        public async Task<Result<TripStartResultDto>> Handle(
            StartTripCommand request,
            CancellationToken cancellationToken)
        {
            // استخدام IUnitOfWork يضمن الوصول للـ Entities من الـ Domain
            var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);
            if (trip == null)
                return Result<TripStartResultDto>.Failure($"Trip with ID {request.TripId} not found", "NotFound");

            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
            if (driver == null)
                return Result<TripStartResultDto>.Failure($"Driver with ID {request.DriverId} not found", "NotFound");

            if (driver.Status == CurrentStatus.On_Trip)
                return Result<TripStartResultDto>.Failure("Driver is already on another trip.", "Conflict");

            var startedAt = DateTime.UtcNow;
            trip.StartedAt = startedAt;
            trip.DriverId = request.DriverId;

            if (request.PickupLocationId > 0) trip.PickupLocationId = request.PickupLocationId;
            if (request.DropoffLocationId > 0) trip.DropoffLocationId = request.DropoffLocationId;

            driver.Status = CurrentStatus.On_Trip;

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.First();
                await entry.ReloadAsync(cancellationToken);

                return Result<TripStartResultDto>.Failure(
                    "Concurrency conflict detected. Please try again.",
                    "ConcurrencyConflict");
            }

            var resultDto = _mapper.Map<TripStartResultDto>(trip);
            resultDto.Status = "Ongoing";

            return Result<TripStartResultDto>.Success(resultDto, "Trip started successfully");
        }
    }
}