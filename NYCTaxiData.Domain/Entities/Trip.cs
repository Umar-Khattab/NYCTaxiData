using System;
using System.Collections.Generic;

namespace NYCTaxiData.Domain.Entities;

public partial class Trip
{
    public int TripId { get; set; }

    public int? SimulationId { get; set; }

    public Guid? DriverId { get; set; }

    public int? PickupLocationId { get; set; }

    public int? DropoffLocationId { get; set; }

    public decimal FareAmount { get; set; }

    public decimal? TipAmount { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public string? CvDataPath { get; set; }

    public string? ProcessStatus { get; set; }

    public virtual Driver? Driver { get; set; }

    public virtual Location? DropoffLocation { get; set; }

    public virtual Location? PickupLocation { get; set; }

    public virtual Simulationrequest? Simulation { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
