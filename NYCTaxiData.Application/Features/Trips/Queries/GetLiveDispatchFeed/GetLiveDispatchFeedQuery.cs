using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed
{
    public record GetLiveDispatchFeedQuery(
        int Limit = 20,
        int MinutesWindow = 60
    ) : IRequest<LiveDispatchFeedResultDto>, ISecureRequest
    {
    }

    public class LiveDispatchFeedResultDto
    {
        public List<DispatchFeedItemDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public DateTime RetrievedAt { get; set; }
    }

    public class DispatchFeedItemDto
    {
        public string DispatchId { get; set; } = string.Empty;
        public int TripId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string PickupZone { get; set; } = string.Empty;
        public string DropoffZone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DispatchedAt { get; set; }
        public string TimeElapsed { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}