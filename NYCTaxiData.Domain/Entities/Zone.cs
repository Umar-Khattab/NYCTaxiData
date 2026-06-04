using NYCTaxiData.Domain.Entities;
using System;
using System.Collections.Generic;

namespace  NYCTaxiData.Domain.Entities;

public partial class Zone
{
    public int ZoneId { get; set; }

    public string? ZoneName { get; set; }

    public long? OsmId { get; set; }

    public double? CenterLat { get; set; }

    public double? CenterLong { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
