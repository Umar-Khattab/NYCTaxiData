using System;

namespace NYCTaxiData.Application.DTOs.Trip
{
    public class DriverActivityDto
    {
        public Guid DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public int TotalTrips { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal AverageRating { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
    }
}
