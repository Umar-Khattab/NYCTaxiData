namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneStatisticsDto(
    int ZoneId,
    string ZoneName,
    int TotalTrips,
    int PickupTrips,
    int DropoffTrips,
    decimal TotalRevenue,
    decimal AverageFare,
    decimal AverageTip);
