using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record GoalProgressDto(
    [property: JsonPropertyName("current")] double Current,
    [property: JsonPropertyName("target")] double Target
);
}
