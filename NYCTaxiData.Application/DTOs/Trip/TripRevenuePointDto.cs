namespace NYCTaxiData.Application.DTOs.Trip;

public sealed record TripRevenuePointDto(
    DateTime Date,
    int TripCount,
    decimal TotalRevenue);
