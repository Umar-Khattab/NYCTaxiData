using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure.Domain.EntitiesAi;

public partial class Etum
{
    public int? PuLocationId { get; set; }

    public int? DoLocationId { get; set; }

    public decimal? TempC { get; set; }

    public decimal? RainMm { get; set; }

    public int? WeatherCode { get; set; }

    public decimal? DistanceProxy { get; set; }

    public int? PickupHour { get; set; }

    public int? PickupDow { get; set; }

    public int? PickupMonth { get; set; }

    public int? PickupMinute { get; set; }

    public int? IsWeekend { get; set; }

    public int? IsRushHour { get; set; }

    public DateTime? Pickup15minBucket { get; set; }

    public string? DistanceBucketLabel { get; set; }

    public decimal? DurationSec { get; set; }

    public decimal? OdHourMedianDuration { get; set; }

    public decimal? PuHourSlowdownIndex { get; set; }

    public int? DistMedianDuration { get; set; }
}
