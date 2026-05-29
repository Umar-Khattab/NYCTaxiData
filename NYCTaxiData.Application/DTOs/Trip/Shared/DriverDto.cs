namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class DriverDto
{
    public Guid DriverId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal? Rating { get; set; }
    public string Status { get; set; } = string.Empty;
}
