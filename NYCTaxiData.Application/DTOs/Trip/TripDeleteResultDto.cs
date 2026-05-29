namespace NYCTaxiData.Application.DTOs.Trip;

public sealed record TripDeleteResultDto(
    int TripId,
    DateTime? DeletedAt,
    string? DeletedBy);
