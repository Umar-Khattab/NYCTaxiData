namespace NYCTaxiData.Application.DTOs.Trip;

public sealed record TripPeakHourDto(
    int Hour,
    int TripCount);
