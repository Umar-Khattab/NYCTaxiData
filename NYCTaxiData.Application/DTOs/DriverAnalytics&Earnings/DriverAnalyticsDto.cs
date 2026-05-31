using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record DriverAnalyticsDto(
     [property: JsonPropertyName("weeklySummary")] WeeklySummaryDto WeeklySummary,
     [property: JsonPropertyName("earningsTrend")] List<double> EarningsTrend,
     [property: JsonPropertyName("peakHours")] List<PeakHourDto> PeakHours,
     [property: JsonPropertyName("topRoutes")] List<TopRouteDto> TopRoutes,
     [property: JsonPropertyName("weeklyGoals")] WeeklyGoalsDto WeeklyGoals
 );
}
