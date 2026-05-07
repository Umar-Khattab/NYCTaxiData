using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Entities
{
    public class DailyStats
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalTrips { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveDrivers { get; set; }
        public double AvgTripMinutes { get; set; }
        public decimal AvgFare { get; set; }
        public int CompletedTrips { get; set; }
        public int CancelledTrips { get; set; }
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }
}
