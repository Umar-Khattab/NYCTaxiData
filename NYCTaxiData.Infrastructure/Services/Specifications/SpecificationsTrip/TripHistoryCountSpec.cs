using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
{
    public class TripHistoryCountSpec : BaseSpecification<Trip>
    {
        public TripHistoryCountSpec() { }

        public TripHistoryCountSpec(Guid driverId)
            : base(t => t.DriverId == driverId)
        {
        }
    }
}
