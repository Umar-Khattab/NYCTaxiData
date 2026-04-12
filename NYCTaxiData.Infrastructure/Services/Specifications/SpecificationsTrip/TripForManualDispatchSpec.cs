using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
{
    public class TripForManualDispatchSpec : BaseSpecification<Trip>
    {
        public TripForManualDispatchSpec(int tripId)
            : base(t => t.TripId == tripId && t.DriverId == null)
        {
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);
        }
    }
}
