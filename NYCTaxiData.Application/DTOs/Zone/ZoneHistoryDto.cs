using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneHistoryDto
    {
        public DateTime Date { get; set; }
        public int TotalTrips { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageFare { get; set; }
        public int PeakHour { get; set; }
    }
}
