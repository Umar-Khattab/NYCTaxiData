namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class ZoneDto
{
    public int ZoneId { get; set; }
    public string ZoneName { get; set; } = string.Empty;
    public string? Borough { get; set; }
    public string? ServiceZone { get; set; }
}
