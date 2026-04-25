namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;

public sealed record DriverDto(
    Guid DriverId,
    string FullName,
    string PlateNumber,
    string LicenseNumber,
    decimal? Rating,
    string Status);
