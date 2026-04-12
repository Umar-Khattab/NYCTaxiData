using MediatR;
using System.Linq.Expressions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed
{
    public class GetLiveDispatchFeedQueryHandler(IUnitOfWork _unitOfWork)
        : IRequestHandler<GetLiveDispatchFeedQuery, LiveDispatchFeedResultDto>
    {
        public async Task<LiveDispatchFeedResultDto> Handle(
            GetLiveDispatchFeedQuery request,
            CancellationToken cancellationToken)
        {
            // Calculate the time window for recent trips
            var cutoffTime = DateTime.UtcNow.AddMinutes(-request.MinutesWindow);

            // Get recent trips within the time window, ordered by most recent first
            var recentTrips = await _unitOfWork.Trips.FindByConditionWithIncludesAsync(
                predicate: t => t.StartedAt >= cutoffTime,
                includes: new Expression<Func<Trip, object>>[]
                {
                    t => t.Driver!,
                    t => t.PickupLocation!,
                    t => t.DropoffLocation!
                });

            // Apply limit and convert to DTOs
            var dispatchItems = recentTrips
                .OrderByDescending(t => t.StartedAt)
                .Take(request.Limit)
                .Select(trip =>
                {
                    var dispatchedAt = trip.StartedAt ?? DateTime.UtcNow;
                    var timeElapsed = FormatTimeElapsed(dispatchedAt);
                    var status = DetermineDispatchStatus(trip);

                    return new DispatchFeedItemDto
                    {
                        DispatchId = GenerateDispatchId(trip),
                        TripId = trip.TripId,
                        DriverName = trip.Driver?.Fullname ?? "Unknown Driver",
                        PickupZone = trip.PickupLocation?.Zone?.ZoneName ?? "Unknown Zone",
                        DropoffZone = trip.DropoffLocation?.Zone?.ZoneName ?? "Unknown Zone",
                        Status = status,
                        DispatchedAt = dispatchedAt,
                        TimeElapsed = timeElapsed,
                        StartedAt = trip.StartedAt,
                        EndedAt = trip.EndedAt
                    };
                })
                .ToList();

            return new LiveDispatchFeedResultDto
            {
                Items = dispatchItems,
                TotalCount = dispatchItems.Count,
                RetrievedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Generates a unique dispatch ID for a trip
        /// </summary>
        private static string GenerateDispatchId(Trip trip)
        {
            var unixTimestamp = trip.StartedAt.HasValue
                ? new DateTimeOffset(trip.StartedAt.Value).ToUnixTimeSeconds()
                : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"DSP-{trip.TripId:D6}-{unixTimestamp}";
        }

        /// <summary>
        /// Determines the current status of a dispatch based on trip state
        /// </summary>
        private static string DetermineDispatchStatus(Trip trip)
        {
            if (trip.EndedAt.HasValue)
                return "Completed";

            if (trip.StartedAt.HasValue && DateTime.UtcNow.Subtract(trip.StartedAt.Value).TotalMinutes > 60)
                return "In-Progress (Long)";

            if (trip.StartedAt.HasValue)
                return "In-Progress";

            return "Pending";
        }

        /// <summary>
        /// Formats elapsed time in human-readable format
        /// </summary>
        private static string FormatTimeElapsed(DateTime dispatchedAt)
        {
            var elapsed = DateTime.UtcNow - dispatchedAt;

            if (elapsed.TotalSeconds < 60)
                return $"{(int)elapsed.TotalSeconds} secs ago";

            if (elapsed.TotalMinutes < 60)
                return $"{(int)elapsed.TotalMinutes} mins ago";

            if (elapsed.TotalHours < 24)
                return $"{(int)elapsed.TotalHours} hours ago";

            return $"{(int)elapsed.TotalDays} days ago";
        }
    }
}
