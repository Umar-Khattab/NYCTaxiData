namespace NYCTaxiData.Application.DTOs.Trip;

public sealed record TripDemandPointDto(
    DateTime Date,
    int TripCount);
