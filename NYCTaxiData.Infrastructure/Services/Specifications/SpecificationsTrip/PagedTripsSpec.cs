using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
{
    public class PagedTripsSpec : BaseSpecification<Trip>
    {
        public PagedTripsSpec(Guid? driverId, int page, int limit)
        {
            if (driverId.HasValue)
                AddCriteria(t => t.DriverId == driverId.Value);

            AddInclude(t => t.Driver!);
            AddOrderByDescending(t => t.StartedAt!);
            ApplyPaging((page - 1) * limit, limit);
        }
    }
}
