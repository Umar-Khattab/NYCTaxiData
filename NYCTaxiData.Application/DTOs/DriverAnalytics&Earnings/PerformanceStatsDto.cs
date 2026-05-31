using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record PerformanceStatsDto(
        [property: JsonPropertyName("avg_per_trip")] double AvgPerTrip,
        [property: JsonPropertyName("earnings_per_hour")] double EarningsPerHour,
        [property: JsonPropertyName("trend")] string Trend
    );
}
