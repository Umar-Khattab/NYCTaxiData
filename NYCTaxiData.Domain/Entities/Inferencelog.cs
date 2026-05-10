using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Inferencelog
{
    public Guid InferenceId { get; set; }

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

    public double? RainMm { get; set; }

    public decimal? RevLag16h { get; set; }

    public decimal? RevLag1Week { get; set; }

    public decimal? RevRollingMean30d { get; set; }

    public decimal? RevRollingMean7d { get; set; }

    public double? RollingMean24h { get; set; }

    public double? TempC { get; set; }

    public decimal? TipRate { get; set; }

    public int? WeatherCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Zone? PuLocation { get; set; }

    public virtual ICollection<Simulationrequest> Simulationrequests { get; set; } = new List<Simulationrequest>();
}
