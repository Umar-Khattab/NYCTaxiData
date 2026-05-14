using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure.Domain.EntitiesAi;

public partial class Revenuefeature
{
    public int? PuLocationId { get; set; }

    public decimal? AvgFare { get; set; }

    public int? DayOfWeek { get; set; }

    public int? IsHoliday { get; set; }

    public int? IsRain { get; set; }

    public int? IsWeekend { get; set; }

    public int? Lag16h { get; set; }

    public int? Lag26h { get; set; }

    public int? Lag46h { get; set; }

    public int? PickupHour { get; set; }

    public decimal? RainMm { get; set; }

    public decimal? RevLag16h { get; set; }

    public decimal? RevLag1Week { get; set; }

    public decimal? RevRollingMean30d { get; set; }

    public decimal? RevRollingMean7d { get; set; }

    public decimal? RollingMean24h { get; set; }

    public decimal? TempC { get; set; }

    public decimal? TipRate { get; set; }

    public int? WeatherCode { get; set; }
}
