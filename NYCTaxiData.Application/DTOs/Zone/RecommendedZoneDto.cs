using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class RecommendedZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Borough { get; set; } = string.Empty;
        public decimal RecommendationScore { get; set; }
        public decimal AvgFare { get; set; }
        public decimal AvgTip { get; set; }
        public decimal DemandSupplyRatio { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
