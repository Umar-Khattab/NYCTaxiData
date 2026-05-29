namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneTrendPointDto(
    DateTime Date,
    int TripCount,
    decimal TotalRevenue);
