namespace NYCTaxiData.Application.DTOs.Trip
{
    public class TripPeakHoursDto
    {
        public int Hour { get; set; }
        
        // Calculated (Historical)
        public int CalculatedTripCount { get; set; }
        public decimal CalculatedTotalRevenue { get; set; }
        
        // Predicted (ML)
        public double PredictedTripCount { get; set; }
        public decimal PredictedTotalRevenue { get; set; }
        
        // Legacy Support
        public int TripCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
