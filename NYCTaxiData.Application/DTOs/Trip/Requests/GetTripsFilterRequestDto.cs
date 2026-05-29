namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class GetTripsFilterRequestDto
{
    public Guid? DriverId { get; set; }
    public int? ZoneId { get; set; }
    public string? Status { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
