using System.Collections.Generic;

namespace NYCTaxiData.Application.DTOs.Trip
{
    public class RevenueStatisticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalFareAmount { get; set; }
        public decimal TotalTipAmount { get; set; }
        public decimal AvgTipPercentage { get; set; }
        public List<RevenuePeriodPointDto> TimeSeriesData { get; set; } = new();
    }

    public class RevenuePeriodPointDto
    {
        public string PeriodLabel { get; set; } = string.Empty; // e.g., "2026-05-29" or "14:00"
        public decimal Revenue { get; set; }
        public decimal FareAmount { get; set; }
        public decimal TipAmount { get; set; }
    }
}
