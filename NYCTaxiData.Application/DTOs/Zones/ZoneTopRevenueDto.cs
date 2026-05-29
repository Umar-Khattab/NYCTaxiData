namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneTopRevenueDto(
    int ZoneId,
    string ZoneName,
    decimal TotalRevenue);
