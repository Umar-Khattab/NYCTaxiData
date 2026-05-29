namespace NYCTaxiData.Application.DTOs.Trip
{
    public class TripTrendDto
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public int TripCount { get; set; }
        public decimal AverageFare { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
