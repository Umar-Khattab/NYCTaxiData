namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneHistoryPointDto(
    DateTime Date,
    int TripCount,
    decimal AverageFare,
    decimal TotalRevenue);
