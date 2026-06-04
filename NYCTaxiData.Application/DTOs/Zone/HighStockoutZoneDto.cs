using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class HighStockoutZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        
        [Obsolete("Borough is obsolete and no longer exists in the zones schema.")]
        public string Borough { get; set; } = string.Empty;

        public long? OsmId { get; set; }
        public double? CenterLatitude { get; set; }
        public double? CenterLongitude { get; set; }
        
        // Predictions
        public double StockoutPrediction { get; set; }
        
        // Calculated (Historical / Current State)
        public int CalculatedDeficit { get; set; }
        public double CalculatedStockoutProbability { get; set; }
        
        // Predicted (ML)
        public int PredictedDeficit { get; set; }
        public double PredictedStockoutProbability { get; set; }
        
        // Legacy Support
        public int PickupCount { get; set; }
        public int AvailableDriversCount { get; set; }
        public int DeficitCount { get; set; }
        public double StockoutProbability { get; set; }
    }
}
