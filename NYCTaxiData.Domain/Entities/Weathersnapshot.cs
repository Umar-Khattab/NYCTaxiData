using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Weathersnapshot
{
    public int WeatherId { get; set; }

    public double? TempC { get; set; }

    public double? RainMm { get; set; }

    public double? IsRain { get; set; }

    public int? WeatherCode { get; set; }

    public DateTime CapturedAt { get; set; }

    public virtual ICollection<Simulationrequest> Simulationrequests { get; set; } = new List<Simulationrequest>();
}
