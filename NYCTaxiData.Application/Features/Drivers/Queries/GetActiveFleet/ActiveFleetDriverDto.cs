namespace NYCTaxiData.Application.Features.Drivers.Queries.GetActiveFleet;

public sealed record ActiveFleetDriverDto(
    Guid DriverId,
    string FullName,
    string PlateNumber,
    decimal? Rating,
    string Status);
