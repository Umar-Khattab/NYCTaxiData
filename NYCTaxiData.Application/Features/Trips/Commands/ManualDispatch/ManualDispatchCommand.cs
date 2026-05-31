using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.DTOs.Trip;
using System;

namespace NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch
{ 
    public record ManualDispatchCommand(
        Guid DriverId,
        int PickupZoneId,
        int DropoffZoneId,
        string PassengerName,
        string PassengerPhone,
        string Priority = "NORMAL",         
        bool SmartRoutingEnabled = true,
        int? TripId = null                  
    ) : IRequest<Result<DispatchResultDto>>, ITransactionalCommand, ISecureRequest;
     
    public class DispatchResultDto
    {
        public string DispatchId { get; set; } = string.Empty;
        public Guid DriverId { get; set; }
        public int PickupZoneId { get; set; }
        public int DropoffZoneId { get; set; }
        public string Status { get; set; } = "Dispatched";
        public DateTime DispatchedAt { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}