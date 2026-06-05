using System.Collections.Generic;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneInsightsDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public double AvgWaitTimeMinutes { get; set; }
        public string PeakPeriodName { get; set; } = string.Empty; // e.g. "Morning Rush"
        public decimal DriverEfficiencyScore { get; set; }
    }
}
