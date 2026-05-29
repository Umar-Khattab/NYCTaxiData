namespace NYCTaxiData.Application.DTOs.Zones;

public sealed record ZoneComparisonResultDto(
    ZoneComparisonItemDto ZoneA,
    ZoneComparisonItemDto ZoneB);
