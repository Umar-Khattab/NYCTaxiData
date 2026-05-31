using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record TopRouteDto(
     [property: JsonPropertyName("route_name")] string RouteName,
     [property: JsonPropertyName("trips_count")] int TripsCount,
     [property: JsonPropertyName("fare")] double Fare,
     [property: JsonPropertyName("status")] string Status
 );
}
