using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Location
{
    public int LocationId { get; set; }

    public int? ZoneId { get; set; }

    public virtual ICollection<Simulationrequest> SimulationrequestDropoffLocations { get; set; } = new List<Simulationrequest>();

    public virtual ICollection<Simulationrequest> SimulationrequestPickupLocations { get; set; } = new List<Simulationrequest>();

    public virtual ICollection<Trip> TripDropoffLocations { get; set; } = new List<Trip>();

    public virtual ICollection<Trip> TripPickupLocations { get; set; } = new List<Trip>();

    public virtual Zone? Zone { get; set; }
}
