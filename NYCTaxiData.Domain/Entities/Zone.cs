using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Zone
{
    public int ZoneId { get; set; }

    public string ZoneName { get; set; } = null!;

    public string? Borough { get; set; }

    public string? ServiceZone { get; set; }

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
}
