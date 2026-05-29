namespace NYCTaxiData.Application.DTOs.Zone
{
    public class TopDemandZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Borough { get; set; } = string.Empty;
        public int PickupCount { get; set; }
        public double PercentageOfTotal { get; set; }
    }
}
