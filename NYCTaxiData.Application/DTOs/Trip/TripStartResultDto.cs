using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Trip
{
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
