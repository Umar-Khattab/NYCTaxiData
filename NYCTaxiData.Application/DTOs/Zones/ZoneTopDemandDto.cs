namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneTopDemandDto(
    int ZoneId,
    string ZoneName,
    int TripCount);
