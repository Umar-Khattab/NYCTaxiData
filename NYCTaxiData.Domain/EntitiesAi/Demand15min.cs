using System;
using System.Collections.Generic;

namespace NYCTaxiData.Domain.EntitiesAi;

public partial class Demand15min
{
    public int? PuLocationId { get; set; }
    public int? PickupCnt { get; set; }
    public int? Lag1 { get; set; }
    public int? Lag4 { get; set; }
    public int? Lag96 { get; set; }
    public decimal? RollMean1h { get; set; }
    public decimal? RollMean3h { get; set; }
    public int? Hour { get; set; }
    public int? Minute { get; set; }
    public int? DayOfWeek { get; set; }
    public int? IsWeekend { get; set; }
    public int? Month { get; set; }
    public decimal? TempC { get; set; }
    public decimal? RainMm { get; set; }
    public int? IsRain { get; set; }
    public int? WeatherCode { get; set; }
}
