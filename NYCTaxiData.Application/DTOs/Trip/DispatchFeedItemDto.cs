using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Trip
{
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
