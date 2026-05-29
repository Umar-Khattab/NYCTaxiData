namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneHeatmapPointDto(
    int ZoneId,
    string ZoneName,
    int PickupTrips,
    int DropoffTrips,
    int TotalTrips);
