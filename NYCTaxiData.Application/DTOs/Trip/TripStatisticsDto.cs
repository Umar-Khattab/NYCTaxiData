namespace NYCTaxiData.Application.DTOs.Trip;

public sealed record TripStatisticsDto(
    int TotalTrips,
    decimal TotalRevenue,
    decimal AverageFare,
    decimal AverageTip,
    decimal AverageTotalAmount,
    double AverageDurationMinutes);
