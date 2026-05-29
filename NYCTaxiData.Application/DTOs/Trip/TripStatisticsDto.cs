namespace NYCTaxiData.Application.DTOs.Trip
{
    public class TripStatisticsDto
    {
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int OngoingTrips { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgFareAmount { get; set; }
        public decimal AvgTipAmount { get; set; }
        public double AverageDurationMinutes { get; set; }
    }
}
