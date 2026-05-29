namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class GetDemandStatisticsRequestDto
{
    public int? ZoneId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}
