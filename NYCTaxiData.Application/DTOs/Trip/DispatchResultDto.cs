using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Trip
{
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
