using MediatR;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory
{
    public class GetTripHistoryQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetTripHistoryQuery, TripHistoryResultDto>
    {
        public async Task<TripHistoryResultDto> Handle(
            GetTripHistoryQuery request,
            CancellationToken cancellationToken)
        {
            // Verify driver exists
            var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);

            if (driver == null)
                throw new NotFoundException($"Driver with ID {request.DriverId} not found");

            // Get paginated trips for the driver
            var (trips, totalCount) = await _unitOfWork.Trips.GetPagedAsync(
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                predicate: t => t.DriverId == request.DriverId,
                orderBy: q => q.OrderByDescending(t => t.StartedAt));

            if (!trips.Any())
            {
                return new TripHistoryResultDto
                {
                    CurrentPage = request.PageNumber,
                    TotalPages = 0,
                    TotalCount = 0,
                    Items = []
                };
            }

            // Map trips to DTOs with zone information
            var tripItems = new List<TripHistoryItemDto>();

            foreach (var trip in trips)
            {
                var pickupZone = trip.PickupLocation?.Zone?.ZoneName ?? "Unknown Zone";
                var dropoffZone = trip.DropoffLocation?.Zone?.ZoneName ?? "Unknown Zone";

                var durationMinutes = 0;
                if (trip.StartedAt.HasValue && trip.EndedAt.HasValue)
                {
                    durationMinutes = (int)(trip.EndedAt.Value - trip.StartedAt.Value).TotalMinutes;
                }
                else if (trip.StartedAt.HasValue)
                {
                    durationMinutes = (int)(DateTime.UtcNow - trip.StartedAt.Value).TotalMinutes;
                }

                var status = trip.EndedAt.HasValue ? "Completed" : "In-Progress";

                tripItems.Add(new TripHistoryItemDto
                {
                    TripId = trip.TripId,
                    PickupZone = pickupZone,
                    DropoffZone = dropoffZone,
                    TotalFare = trip.ActualFare,
                    DurationMinutes = durationMinutes,
                    StartedAt = trip.StartedAt ?? DateTime.UtcNow,
                    EndedAt = trip.EndedAt,
                    Status = status
                });
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new TripHistoryResultDto
            {
                CurrentPage = request.PageNumber,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Items = tripItems
            };
        }
    }
}
