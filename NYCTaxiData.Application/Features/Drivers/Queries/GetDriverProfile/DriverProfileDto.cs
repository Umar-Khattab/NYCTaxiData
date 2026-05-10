namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;

public sealed class DriverProfileDto
{
    public Guid DriverId { get; set; } // تغيير init لـ set
    public string FullName { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal? Rating { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int CompletedTrips { get; set; }
    public int ActiveTrips { get; set; }
    public decimal TotalEarnings { get; set; }
    public DateTime? LastTripEndedAt { get; set; }
}