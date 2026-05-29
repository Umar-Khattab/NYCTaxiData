namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneInsightDto(
    int ZoneId,
    string ZoneName,
    int TotalTrips,
    decimal TotalRevenue,
    decimal AverageFare,
    decimal AverageTip,
    int ActiveDrivers,
    int? PeakHour);
