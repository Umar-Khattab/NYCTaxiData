using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class HeatmapDataPointDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public double? CenterLatitude { get; set; }
        public double? CenterLongitude { get; set; }
        
        [Obsolete("Latitude is obsolete; use CenterLatitude instead.")]
        public double Latitude { get; set; }

        [Obsolete("Longitude is obsolete; use CenterLongitude instead.")]
        public double Longitude { get; set; }
        
        public long? OsmId { get; set; }
        
        // Calculated Density
        public int CalculatedTripCount { get; set; }
        
        // Predicted Density & Stockout Risk (from FastAPI)
        public double PredictedTripCount { get; set; }
        public double PredictedStockoutProbability { get; set; }
        
        // Dynamic ML-based Surge and Demand Level indicators
        public decimal SurgeMultiplier { get; set; }
        public string DemandLevel { get; set; } = "NORMAL"; // LOW, NORMAL, ELEVATED, CRITICAL
        
        // Legacy Support
        public int TripCount { get; set; }
    }
}
