using System;

namespace NYCTaxiData.Application.DTOs.Trip
{
    public class TripDto
    {
        public int TripId { get; set; }
        public Guid? DriverId { get; set; }
        public int? PickupLocationId { get; set; }
        public int? DropoffLocationId { get; set; }
        public decimal FareAmount { get; set; }
        public decimal? TipAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? CvDataPath { get; set; }
        public string? CreatedBy { get; set; }
        public string? ProcessStatus { get; set; }
        public string PickupZoneName { get; set; } = string.Empty;
        public string DropoffZoneName { get; set; } = string.Empty;
    }
}
