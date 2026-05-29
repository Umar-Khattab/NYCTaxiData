namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneMetadataDto(
    int ZoneId,
    string ZoneName,
    string? Borough,
    string? ServiceZone,
    int LocationCount,
    int PickupTrips,
    int DropoffTrips,
    int TotalTrips);
