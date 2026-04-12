using MediatR;
using AutoMapper;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Commands.EndTrip
{
    public class EndTripCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
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

            // Calculate total fare
            var totalFare = request.BaseFare * request.SurgeMultiplier;

            // Update trip with end time and fare
            trip.EndedAt = DateTime.UtcNow;
            trip.ActualFare = totalFare;

            await _unitOfWork.Trips.UpdateAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            var result = _mapper.Map<TripEndResultDto>(trip);
            result.BaseFare = request.BaseFare;
            result.SurgeMultiplier = request.SurgeMultiplier;

            return result;
        }
    }
}