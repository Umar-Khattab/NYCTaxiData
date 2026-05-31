using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record WeeklyGoalsDto(
       [property: JsonPropertyName("earnings_goal")] GoalProgressDto EarningsGoal,
       [property: JsonPropertyName("trips_goal")] GoalProgressDto TripsGoal
   );
}
