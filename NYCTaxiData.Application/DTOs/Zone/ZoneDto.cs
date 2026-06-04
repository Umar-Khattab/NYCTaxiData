using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        
        [Obsolete("Borough is obsolete and no longer exists in the zones schema.")]
        public string? Borough { get; set; }
        
        [Obsolete("ServiceZone is obsolete and no longer exists in the zones schema.")]
        public string? ServiceZone { get; set; }

        public long? OsmId { get; set; }
        public double? CenterLatitude { get; set; }
        public double? CenterLongitude { get; set; }
    }
}
