using MediatR;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Commands.EndTrip
{
    public class EndTripCommandHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<EndTripCommand, TripEndResultDto>
    {
        public async Task<TripEndResultDto> Handle(
            EndTripCommand request,
            CancellationToken cancellationToken)
        {
            // Get the trip
            var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);

            if (trip == null)
                throw new NotFoundException($"Trip with ID {request.TripId} not found");

            // Verify trip has been started
            if (trip.StartedAt == null)
                throw new ConflictException("Trip has not been started yet");

            // Verify trip hasn't already ended
            if (trip.EndedAt != null)
                throw new ConflictException("Trip has already ended");

            // Calculate trip duration in minutes
            var duration = DateTime.UtcNow - trip.StartedAt.Value;
            var durationMinutes = (int)duration.TotalMinutes;

            // Calculate total fare
            var totalFare = request.BaseFare * request.SurgeMultiplier;

            // Update trip with end time and fare
            trip.EndedAt = DateTime.UtcNow;
            trip.ActualFare = totalFare;

            await _unitOfWork.Trips.UpdateAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            return new TripEndResultDto
            {
                TripId = trip.TripId,
                DurationMinutes = durationMinutes,
                BaseFare = request.BaseFare,
                SurgeMultiplier = request.SurgeMultiplier,
                TotalFare = totalFare,
                EndedAt = trip.EndedAt.Value,
                Status = "Completed"
            };
        }
    }
}
