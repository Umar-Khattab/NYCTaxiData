namespace NYCTaxiData.Application.DTOs.Trip;

public sealed record TripTrendPointDto(
    DateTime Date,
    int TripCount,
    decimal TotalRevenue,
    decimal AverageFare);
