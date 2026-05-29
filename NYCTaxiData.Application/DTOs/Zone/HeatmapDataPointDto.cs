using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class HeatmapDataPointDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Borough { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TripCount { get; set; }
        public decimal SurgeMultiplier { get; set; }
        public string DemandLevel { get; set; } = "NORMAL"; // LOW, NORMAL, ELEVATED, CRITICAL
    }
}
