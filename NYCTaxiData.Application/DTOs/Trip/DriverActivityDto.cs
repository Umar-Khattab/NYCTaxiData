namespace NYCTaxiData.Application.DTOs.Trip;

public sealed record DriverActivityDto(
    Guid DriverId,
    string DriverName,
    int TripCount,
    decimal TotalRevenue,
    DateTime? LastTripEndedAt);
