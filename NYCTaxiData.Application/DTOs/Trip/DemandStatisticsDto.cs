using System.Collections.Generic;

namespace NYCTaxiData.Application.DTOs.Trip
{
    public class DemandStatisticsDto
    {
        public int TotalTrips { get; set; }
        public string BusiestDayOfWeek { get; set; } = string.Empty;
        public int BusiestHourOfDay { get; set; }
        public List<DemandPeriodPointDto> TimeSeriesData { get; set; } = new();
    }

    public class DemandPeriodPointDto
    {
        public string PeriodLabel { get; set; } = string.Empty;
        public int TripCount { get; set; }
    }
}
