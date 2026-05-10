using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Simulationresult
{
    public int ResultId { get; set; }

    public int? SimulationId { get; set; }

    public double? DemandP50 { get; set; }

    public double? DemandP90 { get; set; }

    public decimal? RevenueP50 { get; set; }

    public decimal? RevenueP90 { get; set; }

    public decimal? StockoutProb { get; set; }

    public DateTime? ComputedAt { get; set; }

    public virtual Simulationrequest? Simulation { get; set; }
}
