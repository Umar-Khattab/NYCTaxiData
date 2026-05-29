using System;

namespace NYCTaxiData.Application.DTOs.Zone
{
    public class ZoneDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string? Borough { get; set; }
        public string? ServiceZone { get; set; }
    }
}
