namespace NYCTaxiData.Application.DTOs.Zone
{
    public class TopRevenueZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Borough { get; set; } = string.Empty;
        
        // Calculated (Historical)
        public decimal CalculatedRevenue { get; set; }
        public double PercentageOfTotalCalculated { get; set; }
        
        // Predicted (ML)
        public decimal PredictedRevenue { get; set; }
        public double PercentageOfTotalPredicted { get; set; }
        
        // Legacy Support
        public decimal TotalRevenue { get; set; }
        public double PercentageOfTotal { get; set; }
    }
}
