namespace NYCTaxiData.Application.Features.Drivers.Queries.GetShiftStatistics;

public sealed class ShiftStatisticsDto
{
    public Guid DriverId { get; init; }
    public DateTime ShiftStartUtc { get; init; }
    public DateTime ShiftEndUtc { get; init; }
    public double HoursActive { get; init; }
    public int TripsCompleted { get; init; }
    public decimal TotalEarnings { get; init; }
    public int IdleTimeMinutes { get; init; }
}
