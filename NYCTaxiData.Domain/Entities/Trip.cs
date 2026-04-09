using NYCTaxiData.Domain.Entities;
using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Trip
{
    public int TripId { get; set; }

    public int? SimulationId { get; set; }

    public Guid? DriverId { get; set; }

    public int? PickupLocationId { get; set; }

    public int? DropoffLocationId { get; set; }

    public decimal? ActualFare { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public virtual Driver? Driver { get; set; }

    public virtual Location? DropoffLocation { get; set; }

    public virtual Location? PickupLocation { get; set; }

    public virtual Simulationrequest? Simulation { get; set; }
}
