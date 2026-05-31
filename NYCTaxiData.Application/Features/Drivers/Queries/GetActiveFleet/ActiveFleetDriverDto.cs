namespace NYCTaxiData.Application.Features.Drivers.Queries.GetActiveFleet;

public sealed record ActiveFleetDriverDto(
    Guid DriverId,
    string FullName,
    string PlateNumber,
    decimal? Rating,
    string Status)
{
    // 🚀 الحركة دي بتسمح للـ AutoMapper ينشئ الكائن بـ default values
    public ActiveFleetDriverDto() : this(default, string.Empty, string.Empty, default, string.Empty) { }
}
