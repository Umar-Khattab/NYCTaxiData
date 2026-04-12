using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
{
    public class TripHistorySpec : BaseSpecification<Trip>
    {
        // كل الـ trips مع pagination
        public TripHistorySpec(int page, int limit)
        {
            AddInclude(t => t.Driver!);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);
            AddOrderByDescending(t => t.StartedAt!);
            ApplyPaging((page - 1) * limit, limit);
        }

        // trips لسائق معين مع pagination
        public TripHistorySpec(Guid driverId, int page, int limit)
            : base(t => t.DriverId == driverId)
        {
            AddInclude(t => t.Driver!);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);
            AddOrderByDescending(t => t.StartedAt!);
            ApplyPaging((page - 1) * limit, limit);
        }
    }
}
