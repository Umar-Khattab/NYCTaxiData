namespace NYCTaxiData.Application.DTOs.Trip
{
    public class TripPeakHoursDto
    {
        public int Hour { get; set; }
        public int TripCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
