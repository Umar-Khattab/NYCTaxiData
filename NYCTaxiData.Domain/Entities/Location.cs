using System;
using System.Collections.Generic;

namespace NYCTaxiData.Domain.Entities;

public partial class Location
{
    public int LocationId { get; set; }

    public int? ZoneId { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Trip> TripDropoffLocations { get; set; } = new List<Trip>();

    public virtual ICollection<Trip> TripPickupLocations { get; set; } = new List<Trip>();

    public virtual Zone? Zone { get; set; }
}
