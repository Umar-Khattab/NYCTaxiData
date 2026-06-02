using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneStatisticsDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string? Borough { get; set; }
        
        // Calculated (Historical)
        public ZoneCalculatedStats Calculated { get; set; } = new();
        
        // Predicted (ML)
        public ZonePredictedStats Predicted { get; set; } = new();
        
        // Legacy Support
        public int TotalPickupTrips { get; set; }
        public int TotalDropoffTrips { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgFare { get; set; }
        public decimal AvgTip { get; set; }
        public int BusiestHourOfDay { get; set; }
        public string BusiestDayOfWeek { get; set; } = string.Empty;
    }

    public class ZoneCalculatedStats
    {
        public int TotalPickupTrips { get; set; }
        public int TotalDropoffTrips { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgFare { get; set; }
        public decimal AvgTip { get; set; }
        public int BusiestHourOfDay { get; set; }
        public string BusiestDayOfWeek { get; set; } = string.Empty;
    }

    public class ZonePredictedStats
    {
        public double ExpectedDemand15Min { get; set; }
        public double ExpectedDemand6H { get; set; }
        public decimal ExpectedRevenue6H { get; set; }
        public double StockoutProbability { get; set; }
        public int BusiestHourForecast { get; set; }
    }
}
