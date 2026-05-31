using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record PeakHourDto(
     [property: JsonPropertyName("time_slot")] string TimeSlot,
     [property: JsonPropertyName("trips")] int Trips,
     [property: JsonPropertyName("earnings")] double Earnings,
     [property: JsonPropertyName("percentage")] double Percentage
 );
}
