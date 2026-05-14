using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure.Domain.EntitiesAi;

public partial class Stockoutfeature
{
    public int? ZoneId { get; set; }

    public DateTime? TimeBucket6h { get; set; }

    public int? PickupCount { get; set; }

    public int? DropoffCount { get; set; }

    public int? NetFlow { get; set; }

    public int? Hour { get; set; }

    public int? DayOfWeek { get; set; }

    public int? IsWeekend { get; set; }

    public int? IsHoliday { get; set; }

    public decimal? ActivityRatio { get; set; }

    public decimal? TempC { get; set; }

    public decimal? RainMm { get; set; }

    public int? IsRain { get; set; }

    public int? WeatherCode { get; set; }

    public int? Lag1Pickup { get; set; }

    public int? Lag1Dropoff { get; set; }

    public int? Lag1NetFlow { get; set; }
}
