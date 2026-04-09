using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Simulationresult
{
    public int ResultId { get; set; }

    public int SimulationId { get; set; }

    public decimal? DemandForecastP50 { get; set; }

    public decimal? DemandForecastP90 { get; set; }

    public decimal? EtaP50Sec { get; set; }

    public decimal? EtaP90Sec { get; set; }

    public decimal? ExpectedRevenueP50 { get; set; }

    public decimal? ExpectedRevenueP90 { get; set; }

    public decimal? StockoutProbability { get; set; }

    public decimal? RevenueP50 { get; set; }

    public decimal? RevenueP90 { get; set; }

    public decimal? TargetPickupP50 { get; set; }

    public decimal? TargetPickupP90 { get; set; }

    public bool? CacheHit { get; set; }

    public DateTime? ComputedAt { get; set; }

    public virtual Simulationrequest Simulation { get; set; } = null!;
}
