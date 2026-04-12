using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
{
    public class TripsByDriverSpec : BaseSpecification<Trip>
    {
        public TripsByDriverSpec(Guid driverId)
            : base(t => t.DriverId == driverId)
        {
            AddInclude(t => t.Driver!);
            AddOrderByDescending(t => t.StartedAt!);
        }
    }
}
