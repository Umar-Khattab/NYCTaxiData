namespace NYCTaxiData.Application.DTOs.Trip;

public sealed record TripZoneStatisticsDto(
    int ZoneId,
    string ZoneName,
    int PickupTrips,
    int DropoffTrips,
    int TotalTrips,
    decimal TotalRevenue);
