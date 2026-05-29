namespace NYCTaxiData.Application.DTOs.Zone
{
    public class DriverDistributionDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Borough { get; set; } = string.Empty;
        public int ActiveDriversCount { get; set; }
        public int AvailableDriversCount { get; set; }
        public int OnTripDriversCount { get; set; }
    }
}
