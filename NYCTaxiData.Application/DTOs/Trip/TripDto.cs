namespace NYCTaxiData.Application.DTOs.Trip;

public sealed record TripDto(
    int TripId,
    Guid? DriverId,
    int? PickupLocationId,
    int? DropoffLocationId,
    decimal FareAmount,
    decimal? TipAmount,
    decimal? TotalAmount,
    DateTime? StartedAt,
    DateTime CreatedAt,
    DateTime? EndedAt,
    string? ProcessStatus,
    string? CvDataPath);
