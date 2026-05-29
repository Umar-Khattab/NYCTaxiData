namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class GetTripTrendsRequestDto
{
    public int? ZoneId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int IntervalMinutes { get; set; } = 60;
}
