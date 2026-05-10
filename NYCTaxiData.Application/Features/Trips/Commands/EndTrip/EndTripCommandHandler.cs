using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;

namespace NYCTaxiData.Application.Features.Trips.Commands.EndTrip
{
    public class EndTripCommandHandler(IUnitOfWork _unitOfWork, TaxiDbContext _context, IMapper _mapper)
        : IRequestHandler<EndTripCommand, Result<TripEndResultDto>>
    {
        public async Task<Result<TripEndResultDto>> Handle(
            EndTripCommand request,
            CancellationToken cancellationToken)
        {
            var trip = await _context.Trips
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.TripId == request.TripId, cancellationToken);

            if (trip == null)
                return Result<TripEndResultDto>.Failure($"Trip not found", "NotFound");

            if (trip.StartedAt == default) // تعديل للـ StartedAt لأنها مش nullable في الملف الجديد
                return Result<TripEndResultDto>.Failure("Trip has not been started yet", "Conflict");

            if (trip.EndedAt != null)
                return Result<TripEndResultDto>.Failure("Trip has already ended", "Conflict");

            var endedAt = DateTime.UtcNow;
            var durationMinutes = (endedAt - trip.StartedAt).TotalMinutes;

            // تم تعديل المسمى لـ TotalAmount بناءً على ملف الـ Trip الجديد
            var totalFare = Math.Round(((decimal)durationMinutes * request.FarePerMinute + request.BaseFare) * request.SurgeMultiplier, 2);

            trip.EndedAt = endedAt;
            trip.TotalAmount = totalFare;

            if (trip.Driver != null)
            {
                trip.Driver.Status = CurrentStatus.Available;
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                return Result<TripEndResultDto>.Failure($"Error: {ex.Message}", "InternalError");
            }

            var resultDto = _mapper.Map<TripEndResultDto>(trip);
            resultDto.DurationMinutes = (int)durationMinutes;
            resultDto.TotalFare = totalFare;

            return Result<TripEndResultDto>.Success(resultDto, "Trip ended successfully");
        }
    }
}