using MediatR;
using NYCTaxiData.Application.Common.Plumbing;  
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using System;

namespace NYCTaxiData.Application.Features.Trips.Commands.StartTrip
{
    // ?? «· ⁄œÌ·: ‘Ì· ITransactionalCommand ⁄‘«‰ «·‹ SaveChanges Ì—„Ì ›Ì «·‹ DB „»«‘—… »œÊ‰ Rollback „‰ «·‹ Behavior
    public record StartTripCommand(
        int TripId,
        Guid DriverId,
        int PickupLocationId,
        int DropoffLocationId
    ) : IRequest<Result<TripStartResultDto>>, ISecureRequest; // ?? ‘Ì·‰«Â« „‰ Â‰«
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