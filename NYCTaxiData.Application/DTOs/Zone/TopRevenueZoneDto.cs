using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class TopRevenueZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        
        [Obsolete("Borough is obsolete and no longer exists in the zones schema.")]
        public string Borough { get; set; } = string.Empty;

        public long? OsmId { get; set; }
        public double? CenterLatitude { get; set; }
        public double? CenterLongitude { get; set; }
        
        // Predictions
        public double RevenuePrediction { get; set; }
        
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
