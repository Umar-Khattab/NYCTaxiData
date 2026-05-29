namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneDto(
    int ZoneId,
    string ZoneName,
    string? Borough,
    string? ServiceZone);
