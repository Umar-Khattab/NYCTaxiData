using System.Collections.Generic;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneMetadataDto
    {
        public int TotalZones { get; set; }
        public int TotalBoroughs { get; set; }
        public int TotalServiceZones { get; set; }
        public Dictionary<string, int> BoroughCounts { get; set; } = new();
    }
}
