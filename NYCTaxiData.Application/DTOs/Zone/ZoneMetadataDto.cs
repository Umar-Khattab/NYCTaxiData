using System;
using System.Collections.Generic;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneMetadataDto
    {
        public int TotalZones { get; set; }

        [Obsolete("TotalBoroughs is obsolete and no longer exists in the zones schema.")]
        public int TotalBoroughs { get; set; }

        [Obsolete("TotalServiceZones is obsolete and no longer exists in the zones schema.")]
        public int TotalServiceZones { get; set; }

        [Obsolete("BoroughCounts is obsolete and no longer exists in the zones schema.")]
        public Dictionary<string, int> BoroughCounts { get; set; } = new();

        public int ZonesWithOsmId { get; set; }
        public Dictionary<string, int> OsmIdCounts { get; set; } = new();
    }
}
