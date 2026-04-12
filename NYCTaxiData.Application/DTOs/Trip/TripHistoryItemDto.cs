using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Trip
{
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
