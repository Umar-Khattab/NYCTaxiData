namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class GetRevenueStatisticsRequestDto
{
    public int? ZoneId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}
