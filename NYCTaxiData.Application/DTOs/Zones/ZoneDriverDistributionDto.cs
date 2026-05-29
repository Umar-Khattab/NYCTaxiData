namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneDriverDistributionDto(
    int ZoneId,
    string ZoneName,
    int ActiveDrivers,
    int TripCount);
