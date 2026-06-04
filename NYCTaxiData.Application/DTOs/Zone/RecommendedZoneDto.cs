using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class RecommendedZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        
        [Obsolete("Borough is obsolete and no longer exists in the zones schema.")]
        public string Borough { get; set; } = string.Empty;

        public long? OsmId { get; set; }
        public double? CenterLatitude { get; set; }
        public double? CenterLongitude { get; set; }
        
        // Recommendation scores (Directly from FastAPI repositioning plan / profit plan)
        public decimal RecommendationScore { get; set; }
        public decimal DemandSupplyRatio { get; set; }
        public decimal PredictedRevenueYield { get; set; }
        public string Reason { get; set; } = string.Empty;
        
        // Legacy Support (historical aggregates)
        public decimal AvgFare { get; set; }
        public decimal AvgTip { get; set; }
    }
}
