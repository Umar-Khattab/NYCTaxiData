namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZonePeakHourDto(
    int Hour,
    int TripCount);
