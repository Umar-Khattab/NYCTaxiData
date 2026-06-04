namespace NYCTaxiData.Application.DTOs.Zone
{
    public class DriverDistributionDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty; 
        public double? CenterLat { get; set; }
        public double? CenterLong { get; set; }
        public int ActiveDriversCount { get; set; }
        public int AvailableDriversCount { get; set; }
        public int OnTripDriversCount { get; set; }
    }
}
