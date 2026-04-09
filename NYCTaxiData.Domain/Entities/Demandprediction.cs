using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Demandprediction
{
    public int ZoneId { get; set; }

    public DateTime TimeBucket6h { get; set; }

    public double? PredictedPickupCount { get; set; }

    public double? P50 { get; set; }

    public double? P90 { get; set; }
}
