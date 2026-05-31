using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore; 
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Trips.Commands.EndTrip
{
    public class EndTripCommandHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<EndTripCommand, Result<TripEndResultDto>>
    {
        public async Task<Result<TripEndResultDto>> Handle(
            EndTripCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Ã·» »Ì«‰«  «·—Õ·…
            var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId);

            if (trip == null)
                return Result<TripEndResultDto>.Failure("Trip not found", "NotFound");

            // 2. «· √ﬂœ «·¬„‰ ≈‰ «·—Õ·… »œ√  (›Õ’ «·‹ null «·’—ÌÕ)
            if (trip.StartedAt == null)
                return Result<TripEndResultDto>.Failure("Trip has not been started yet", "Conflict");

            // 3. «· √ﬂœ ≈‰Â« „‰ Â ‘ ﬁ»· ﬂœ…
            if (trip.EndedAt != null)
                return Result<TripEndResultDto>.Failure("Trip has already ended", "Conflict");

            // 4. Õ”«»«  «·Êﬁ  Ê«·‹ Fare
            var endedAt = DateTime.UtcNow;
            var durationMinutes = (endedAt - trip.StartedAt.Value).TotalMinutes;

            //  √„Ì‰ «·Õ”«»«  ·Ê «·—Õ·… « ﬁ›·  ›Ì ‰›” «·œﬁÌﬁ… (⁄·Ï «·√ﬁ· ‰Õ”» œﬁÌﬁ… Ê«Õœ…)
            if (durationMinutes <= 0) durationMinutes = 1;

            var totalFare = Math.Round(((decimal)durationMinutes * request.FarePerMinute + request.BaseFare) * request.SurgeMultiplier, 2);
             
            trip.EndedAt = endedAt;
            trip.TotalAmount = totalFare;
             
            var driver = await _unitOfWork.Drivers.GetByIdAsync(trip.DriverId);
            if (driver != null)
            {
                driver.Status = CurrentStatus.Available;
            }

            try
            { 
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<TripEndResultDto>.Failure($"Database Error: {ex.Message}", "InternalError");
            }
             
            var resultDto = new TripEndResultDto
            {
                TripId = trip.TripId,
                EndedAt = trip.EndedAt.Value,
                DurationMinutes = (int)durationMinutes,
                TotalFare = totalFare
            };

            return Result<TripEndResultDto>.Success(resultDto, "Trip ended successfully");
        }
    }
}