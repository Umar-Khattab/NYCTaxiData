using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneStatisticsDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string? Borough { get; set; }
        public int TotalPickupTrips { get; set; }
        public int TotalDropoffTrips { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgFare { get; set; }
        public decimal AvgTip { get; set; }
        public int BusiestHourOfDay { get; set; }
        public string BusiestDayOfWeek { get; set; } = string.Empty;
    }
}
