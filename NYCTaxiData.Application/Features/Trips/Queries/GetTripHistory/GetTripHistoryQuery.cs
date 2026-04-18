using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory
{
    public record GetTripHistoryQuery(
        Guid DriverId,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<Result<TripHistoryResultDto>>, ISecureRequest
    {
    }

    public class TripHistoryResultDto
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public List<TripHistoryItemDto> Items { get; set; } = [];
    }

    public class TripHistoryItemDto
    {
        public int TripId { get; set; }
        public string PickupZone { get; set; } = string.Empty;
        public string DropoffZone { get; set; } = string.Empty;
        public decimal? TotalFare { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}