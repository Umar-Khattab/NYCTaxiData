using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.DriverAnalytics
{
    public record DailyBreakdownDto(
       [property: JsonPropertyName("day")] string Day,
       [property: JsonPropertyName("amount")] double Amount
   );
}
