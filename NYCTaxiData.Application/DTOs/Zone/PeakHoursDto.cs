namespace NYCTaxiData.Application.DTOs.Zone
{
    public class PeakHoursDto
    {
        public int Hour { get; set; } // 0-23
        public int TripCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageFare { get; set; }
    }
}
