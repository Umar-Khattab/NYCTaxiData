namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;

public sealed class DriverProfileDto
{
    public Guid DriverId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string PlateNumber { get; init; } = string.Empty;
    public string LicenseNumber { get; init; } = string.Empty;
    public decimal? Rating { get; init; }
    public string Status { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int CompletedTrips { get; init; }
    public int ActiveTrips { get; init; }
    public decimal TotalEarnings { get; init; }
    public DateTime? LastTripEndedAt { get; init; }
}
