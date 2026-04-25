using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch
{
    public record ManualDispatchCommand(
    Guid DriverId,
    int PickupZoneId,
    int DropoffZoneId,
    string PassengerName,
    string PassengerPhone
) : MediatR.IRequest<NYCTaxiData.Application.Common.Plumping.Result<DispatchResultDto>>, // المسار الكامل هنا
    ITransactionalCommand,
    ISecureRequest;

    public class DispatchResultDto
    {
        public string DispatchId { get; set; } = string.Empty;
        public Guid DriverId { get; set; }
        public int PickupZoneId { get; set; }
        public int DropoffZoneId { get; set; }
        public string Status { get; set; } = "Sent";
        public DateTime DispatchedAt { get; set; }
        public string PassengerName { get; set; } = string.Empty;
    }
}