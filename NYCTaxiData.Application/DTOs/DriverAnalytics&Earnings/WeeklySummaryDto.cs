using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record WeeklySummaryDto(
     [property: JsonPropertyName("total_earnings")] double TotalEarnings,
     [property: JsonPropertyName("completed_trips")] int CompletedTrips,
     [property: JsonPropertyName("online_hours")] double OnlineHours,
     [property: JsonPropertyName("avg_per_hour")] double AvgPerHour,
     [property: JsonPropertyName("trends")] List<string> Trends
 );
}
