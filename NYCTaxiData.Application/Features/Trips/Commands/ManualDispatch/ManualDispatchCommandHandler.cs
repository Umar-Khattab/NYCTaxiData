using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Tracking;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Data.Contexts;
using IUnitOfWork = NYCTaxiData.Domain.Interfaces.IUnitOfWork;

namespace NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch
{
    public class ManualDispatchCommandHandler(
        IUnitOfWork _unitOfWork,
        IMapper _mapper,
        IDispatchNotificationService _dispatchService,
        TaxiDbContext _context)  
        : IRequestHandler<ManualDispatchCommand, Result<DispatchResultDto>>
    {
        public async Task<Result<DispatchResultDto>> Handle(
            ManualDispatchCommand request,
            CancellationToken cancellationToken)
        {
            var driverId = request.DriverId;
             
            var driver = await _context.Drivers
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == driverId, cancellationToken);

            if (driver == null)
                return Result<DispatchResultDto>.Failure($"Driver with ID {driverId} not found", "NotFound");
             
            if (driver.Status != CurrentStatus.Available)
                return Result<DispatchResultDto>.Failure($"Driver is {driver.Status}. Only Available drivers can be dispatched.", "Conflict");

            var pickupZone = await _unitOfWork.Zones.GetByIdAsync(request.PickupZoneId);
            var dropoffZone = await _unitOfWork.Zones.GetByIdAsync(request.DropoffZoneId);

            if (pickupZone == null || dropoffZone == null)
                return Result<DispatchResultDto>.Failure("Pickup or Dropoff zone not found", "NotFound");

            var pickupLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == request.PickupZoneId);
            var dropoffLocations = await _unitOfWork.Locations.FindByConditionAsync(l => l.ZoneId == request.DropoffZoneId);

            if (!pickupLocations.Any() || !dropoffLocations.Any())
                return Result<DispatchResultDto>.Failure("No locations found in specified zones", "NotFound");

            var trip = new Trip
            {
                DriverId = driverId,
                PickupLocationId = pickupLocations.First().LocationId,
                DropoffLocationId = dropoffLocations.First().LocationId,
                StartedAt = DateTime.UtcNow
            };

            driver.Status = CurrentStatus.On_Trip;

            try
            {
                await _unitOfWork.Trips.AddAsync(trip);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<DispatchResultDto>.Failure("Driver status has changed. Please try again.", "Conflict");
            }
             
            var driverPhone = driver.User?.PhoneNumber ?? "01111128427";

            await _dispatchService.SendDispatchToDriverAsync(driverPhone, new DispatchNotificationDto
            {
                DriverPhone = driverPhone,
                TargetZoneId = request.PickupZoneId.ToString(),
                TargetZoneName = pickupZone.ZoneName ?? "Unknown",
                Priority = request.Priority ?? "High",
                Message = $"New Trip Assigned: From {pickupZone.ZoneName} to {dropoffZone.ZoneName}",
                IssuedAt = DateTime.UtcNow
            }, cancellationToken);

            var resultDto = _mapper.Map<DispatchResultDto>(trip);
            resultDto.PickupZoneId = request.PickupZoneId;
            resultDto.DropoffZoneId = request.DropoffZoneId;
            resultDto.PassengerName = request.PassengerName;
            resultDto.DispatchId = $"DSP-{trip.TripId}";

            return Result<DispatchResultDto>.Success(resultDto, "Manual dispatch completed successfully");
        }
    }
}