using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneTrendDto
    {
        public string TimeLabel { get; set; } = string.Empty; // "Hour 08:00" or "Monday"
        public int TripCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgFare { get; set; }
    }
}
