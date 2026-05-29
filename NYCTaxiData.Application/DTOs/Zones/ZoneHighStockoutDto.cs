namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneHighStockoutDto(
    int ZoneId,
    string ZoneName,
    int TripCount,
    int ActiveDrivers,
    decimal StockoutScore);
