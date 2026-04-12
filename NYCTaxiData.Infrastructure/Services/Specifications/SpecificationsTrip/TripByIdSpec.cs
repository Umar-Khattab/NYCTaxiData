using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
{
    public class TripByIdSpec : BaseSpecification<Trip>
    {
        public TripByIdSpec(int tripId)
            : base(t => t.TripId == tripId)
        {
            AddInclude(t => t.Driver!);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);
        }
    }
}
