using System;
using System.Collections.Generic;

namespace NYCTaxiData.Domain.Entities;

public partial class Simulationrequest
{
    public int SimulationId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? InferenceId { get; set; }

    public int? PickupLocationId { get; set; }

    public int? DropoffLocationId { get; set; }

    public DateTime PickupDatetime { get; set; }

    public int? PassengerCount { get; set; }

    public virtual Location? DropoffLocation { get; set; }

    public virtual Inferencelog? Inference { get; set; }

    public virtual Location? PickupLocation { get; set; }

    public virtual Simulationresult? Simulationresult { get; set; }

    public virtual Trip? Trip { get; set; }

    public virtual User1? User { get; set; }
}
