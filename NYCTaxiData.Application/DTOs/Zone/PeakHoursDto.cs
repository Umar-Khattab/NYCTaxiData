namespace NYCTaxiData.Application.DTOs.Zone
{
    public class PeakHoursDto
    {
        public int Hour { get; set; } // 0-23
        
        // Calculated (Historical)
        public int CalculatedTripCount { get; set; }
        public decimal CalculatedTotalRevenue { get; set; }
        public decimal CalculatedAverageFare { get; set; }
        
        // Predicted (ML)
        public double PredictedTripCount { get; set; }
        public decimal PredictedTotalRevenue { get; set; }
        
        // Legacy Support
        public int TripCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageFare { get; set; }
    }
}
