using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record HeaderSummaryDto(
     [property: JsonPropertyName("total_earnings")] double TotalEarnings,
     [property: JsonPropertyName("trips")] int Trips,
     [property: JsonPropertyName("hours")] double Hours
 );
}
