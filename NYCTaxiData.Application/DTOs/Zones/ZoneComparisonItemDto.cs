namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneComparisonItemDto(
    int ZoneId,
    string ZoneName,
    int TotalTrips,
    decimal TotalRevenue,
    decimal AverageFare);
