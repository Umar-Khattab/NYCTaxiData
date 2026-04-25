// NYCTaxiData.Domain/DTOs/Tracking/DriverLocationInfo.cs
namespace NYCTaxiData.Application.DTOs.Tracking;

public class DriverLocationInfo
{
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Speed { get; set; }
    public double? Heading { get; set; }
    public string Status { get; set; } = "Available";
    public DateTime UpdatedAt { get; set; }
}