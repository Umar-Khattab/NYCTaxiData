using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record DriverEarningsDto(
      [property: JsonPropertyName("headerSummary")] HeaderSummaryDto HeaderSummary,
      [property: JsonPropertyName("performanceStats")] PerformanceStatsDto PerformanceStats,
      [property: JsonPropertyName("dailyBreakdown")] List<DailyBreakdownDto> DailyBreakdown,
      [property: JsonPropertyName("recentTrips")] List<RecentTripDto> RecentTrips
  );
}
