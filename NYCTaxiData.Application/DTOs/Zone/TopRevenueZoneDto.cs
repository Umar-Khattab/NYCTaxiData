namespace NYCTaxiData.Application.DTOs.Zone
{
    public class TopRevenueZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Borough { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public double PercentageOfTotal { get; set; }
    }
}
