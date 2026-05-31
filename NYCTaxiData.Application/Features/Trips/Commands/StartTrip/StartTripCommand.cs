using MediatR;
using NYCTaxiData.Application.Common.Plumping;  
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using System;

namespace NYCTaxiData.Application.Features.Trips.Commands.StartTrip
{
    // 🚀 التعديل: شيل ITransactionalCommand عشان الـ SaveChanges يرمي في الـ DB مباشرة بدون Rollback من الـ Behavior
    public record StartTripCommand(
        int TripId,
        Guid DriverId,
        int PickupLocationId,
        int DropoffLocationId
    ) : IRequest<Result<TripStartResultDto>>, ISecureRequest; // 👈 شيلناها من هنا
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