namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class GetZoneStatisticsRequestDto
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}
