using MediatR;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch
{
    public class ManualDispatchCommandHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<ManualDispatchCommand, DispatchResultDto>
    {
        public async Task<DispatchResultDto> Handle(
            ManualDispatchCommand request,
            CancellationToken cancellationToken)
        {
            // Verify driver exists
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);

            if (driver == null)
                throw new NotFoundException($"Driver with ID {request.DriverId} not found");

            // Verify pickup zone exists
            var pickupZone = await _unitOfWork.Zones.GetByIdAsync(request.PickupZoneId);

            if (pickupZone == null)
                throw new NotFoundException($"Pickup zone with ID {request.PickupZoneId} not found");

            // Verify dropoff zone exists
            var dropoffZone = await _unitOfWork.Zones.GetByIdAsync(request.DropoffZoneId);

            if (dropoffZone == null)
                throw new NotFoundException($"Dropoff zone with ID {request.DropoffZoneId} not found");

            // Get first location from each zone for trip creation
            var pickupLocations = await _unitOfWork.Locations.FindByConditionAsync(
                l => l.ZoneId == request.PickupZoneId);

            if (pickupLocations == null || !pickupLocations.Any())
                throw new NotFoundException($"No locations found in pickup zone {request.PickupZoneId}");

            var dropoffLocations = await _unitOfWork.Locations.FindByConditionAsync(
                l => l.ZoneId == request.DropoffZoneId);

            if (dropoffLocations == null || !dropoffLocations.Any())
                throw new NotFoundException($"No locations found in dropoff zone {request.DropoffZoneId}");

            // Create new trip
            var trip = new Trip
            {
                DriverId = request.DriverId,
                PickupLocationId = pickupLocations.First().LocationId,
                DropoffLocationId = dropoffLocations.First().LocationId,
                StartedAt = null, // Trip hasn't started yet
                EndedAt = null
            };

            await _unitOfWork.Trips.AddAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            // Generate dispatch ID
            var dispatchId = $"DSP-{trip.TripId:D6}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            return new DispatchResultDto
            {
                DispatchId = dispatchId,
                DriverId = request.DriverId,
                PickupZoneId = request.PickupZoneId,
                DropoffZoneId = request.DropoffZoneId,
                Status = "Sent",
                DispatchedAt = DateTime.UtcNow,
                PassengerName = request.PassengerName
            };
        }
    }
}
