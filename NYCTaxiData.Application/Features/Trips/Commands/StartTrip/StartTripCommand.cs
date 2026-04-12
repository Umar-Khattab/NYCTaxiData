using MediatR;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces; 

namespace NYCTaxiData.Application.Features.Trips.Commands.StartTrip
{
    public record StartTripCommand(
        Guid DriverId,
        int PickupLocationId,
        int DropoffLocationId
    ) : IRequest<TripStartResultDto>, ITransactionalCommand, ISecureRequest
    {
    }

    public class TripStartResultDto
    {
        public int TripId { get; set; }
        public Guid DriverId { get; set; }
        public string Status { get; set; } = "In-Progress";
        public DateTime StartedAt { get; set; }
        public int PickupLocationId { get; set; }
        public int DropoffLocationId { get; set; }
    }
}