using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Commands.CreateTrip
{
    public class CreateTripCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<CreateTripCommand, Result<TripDto>>
    {
        public async Task<Result<TripDto>> Handle(
            CreateTripCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Verify driver exists
            var driverExists = await _unitOfWork.Drivers.ExistsAsync(request.DriverId);
            if (!driverExists)
                return Result<TripDto>.Failure($"Driver with ID {request.DriverId} does not exist", "Validation");

            // 2. Verify pickup location exists
            var pickupExists = await _unitOfWork.Locations.ExistsAsync(request.PickupLocationId);
            if (!pickupExists)
                return Result<TripDto>.Failure($"Pickup location with ID {request.PickupLocationId} does not exist", "Validation");

            // 3. Verify dropoff location exists
            var dropoffExists = await _unitOfWork.Locations.ExistsAsync(request.DropoffLocationId);
            if (!dropoffExists)
                return Result<TripDto>.Failure($"Dropoff location with ID {request.DropoffLocationId} does not exist", "Validation");

            // 4. Create Trip Entity
            var trip = new Trip
            {
                DriverId = request.DriverId,
                PickupLocationId = request.PickupLocationId,
                DropoffLocationId = request.DropoffLocationId,
                FareAmount = request.FareAmount,
                TipAmount = request.TipAmount, 
                StartedAt = DateTime.UtcNow.AddMinutes(-20), // Simulation: started 20 mins ago
                EndedAt = DateTime.UtcNow, 
                ProcessStatus = "Completed"
            };

            await _unitOfWork.Trips.AddAsync(trip);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Re-fetch to get navigation properties mapped correctly
            var savedTrip = await _unitOfWork.Trips.GetByIdAsync(trip.TripId);
            var dto = _mapper.Map<TripDto>(savedTrip ?? trip);

            return Result<TripDto>.Success(dto, "Trip created successfully");
        }
    }
}
