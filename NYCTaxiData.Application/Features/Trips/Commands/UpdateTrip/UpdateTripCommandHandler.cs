using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Commands.UpdateTrip
{
    public class UpdateTripCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<UpdateTripCommand, Result<TripDto>>
    {
        public async Task<Result<TripDto>> Handle(
            UpdateTripCommand request,
            CancellationToken cancellationToken)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);
            if (trip == null)
                return Result<TripDto>.Failure($"Trip with ID {request.TripId} not found", "NotFound");

            // Update properties
            trip.FareAmount = request.FareAmount;
            trip.TipAmount = request.TipAmount;
            trip.TotalAmount = request.FareAmount + request.TipAmount;
            trip.ProcessStatus = request.ProcessStatus;

            if (request.ProcessStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) && !trip.EndedAt.HasValue)
            {
                trip.EndedAt = DateTime.UtcNow;
            }

            await _unitOfWork.Trips.UpdateAsync(trip);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Re-fetch to get navigation properties mapped correctly
            var savedTrip = await _unitOfWork.Trips.GetByIdAsync(trip.TripId);
            var dto = _mapper.Map<TripDto>(savedTrip ?? trip);

            return Result<TripDto>.Success(dto, "Trip updated successfully");
        }
    }
}
