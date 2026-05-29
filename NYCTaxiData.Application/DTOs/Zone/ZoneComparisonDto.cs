using System.Collections.Generic;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneComparisonDto
    {
        public List<ZoneStatisticsDto> ComparisonData { get; set; } = new();
        public string HighestRevenueZone { get; set; } = string.Empty;
        public string BusiestZone { get; set; } = string.Empty;
    }
}
