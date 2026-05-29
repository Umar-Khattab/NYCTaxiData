namespace NYCTaxiData.Application.DTOs.Trip;

public sealed class CreateTripRequestDto
{
    public Guid? DriverId { get; set; }
    public int PickupLocationId { get; set; }
    public int DropoffLocationId { get; set; }
    public decimal FareAmount { get; set; }
    public decimal? TipAmount { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}
