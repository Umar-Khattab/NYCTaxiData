using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record RecentTripDto(
     [property: JsonPropertyName("from")] string From,
     [property: JsonPropertyName("to")] string To,
     [property: JsonPropertyName("start_time")] DateTime StartTime,
     [property: JsonPropertyName("duration")] int Duration,
     [property: JsonPropertyName("distance")] double Distance,
     [property: JsonPropertyName("fare")] double Fare
 );
}
