namespace NYCTaxiData.Application.DTOs.Zone
{
    public class HighStockoutZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Borough { get; set; } = string.Empty;
        
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
