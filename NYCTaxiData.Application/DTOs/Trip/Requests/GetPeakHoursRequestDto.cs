namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class GetPeakHoursRequestDto
{
    public int? ZoneId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}
