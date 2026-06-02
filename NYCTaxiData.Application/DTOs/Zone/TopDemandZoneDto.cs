namespace NYCTaxiData.Application.DTOs.Zone
{
    public class TopDemandZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Borough { get; set; } = string.Empty;
        
        // Calculated (Historical)
        public int CalculatedPickups { get; set; }
        public double PercentageOfTotalCalculated { get; set; }
        
        // Predicted (ML)
        public double PredictedPickups { get; set; }
        public double PercentageOfTotalPredicted { get; set; }
        
        // Legacy Support
        public int PickupCount { get; set; }
        public double PercentageOfTotal { get; set; }
    }
}
