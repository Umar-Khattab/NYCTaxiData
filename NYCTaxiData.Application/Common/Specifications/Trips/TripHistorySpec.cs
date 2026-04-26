using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Common.Specifications.Trips;

public sealed class TripHistorySpec : BaseSpecification<Trip>
{
    public TripHistorySpec(Guid? driverId, int page, int limit)
        : base(t => !driverId.HasValue || t.DriverId == driverId.Value)
    {
        AddInclude(t => t.Driver!);
        AddInclude(t => t.PickupLocation!);
        AddInclude(t => t.DropoffLocation!);
        AddOrderByDescending(t => t.StartedAt!);
        ApplyPaging((page - 1) * limit, limit);
    }
}
