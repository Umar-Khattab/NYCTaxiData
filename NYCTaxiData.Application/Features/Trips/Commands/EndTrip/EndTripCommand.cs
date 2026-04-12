using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Trips.Commands.EndTrip
{
    public record EndTripCommand(
        int TripId,
        decimal BaseFare = 2.50m,
        decimal SurgeMultiplier = 1.0m
    ) : IRequest<TripEndResultDto>, ITransactionalCommand, ISecureRequest
    {
    }

    public class TripEndResultDto
    {
        public int TripId { get; set; }
        public int DurationMinutes { get; set; }
        public decimal BaseFare { get; set; }
        public decimal SurgeMultiplier { get; set; }
        public decimal TotalFare { get; set; }
        public DateTime EndedAt { get; set; }
        public string Status { get; set; } = "Completed";
    }
}
