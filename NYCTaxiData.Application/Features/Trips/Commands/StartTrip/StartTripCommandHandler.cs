using MediatR;
using AutoMapper;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Commands.StartTrip
{
    public class StartTripCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<StartTripCommand, TripStartResultDto>
    {
        public async Task<TripStartResultDto> Handle(
            StartTripCommand request,
            CancellationToken cancellationToken)
        {
            // Verify driver exists
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);

            if (driver == null)
                throw new NotFoundException($"Driver with ID {request.DriverId} not found");

            // Verify locations exist
            var pickupLocation = await _unitOfWork.Locations.GetByIdAsync(request.PickupLocationId);

            if (pickupLocation == null)
                throw new NotFoundException($"Pickup location with ID {request.PickupLocationId} not found");

            var dropoffLocation = await _unitOfWork.Locations.GetByIdAsync(request.DropoffLocationId);

            if (dropoffLocation == null)
                throw new NotFoundException($"Dropoff location with ID {request.DropoffLocationId} not found");

            // Create new trip
            var trip = new Trip
            {
                DriverId = request.DriverId,
                PickupLocationId = request.PickupLocationId,
                DropoffLocationId = request.DropoffLocationId,
                StartedAt = DateTime.UtcNow
            };

            await _unitOfWork.Trips.AddAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<TripStartResultDto>(trip);
        }
    }
}
