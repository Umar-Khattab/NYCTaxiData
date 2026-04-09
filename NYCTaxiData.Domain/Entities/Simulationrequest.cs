using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Simulationrequest
{
    public int SimulationId { get; set; }

    public Guid? UserId { get; set; }

    public int? PickupLocationId { get; set; }

    public int? DropoffLocationId { get; set; }

    public int? WeatherId { get; set; }

    public double? PassengerCount { get; set; }

    public DateTime PickupDatetime { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Location? DropoffLocation { get; set; }

    public virtual Location? PickupLocation { get; set; }

    public virtual Simulationresult? Simulationresult { get; set; }

    public virtual Trip? Trip { get; set; }

    public virtual User1? User { get; set; }

    public virtual Weathersnapshot? Weather { get; set; }
}
