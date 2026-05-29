namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class RevenuePointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int? ZoneId { get; set; }
    public int? Hour { get; set; }
}
