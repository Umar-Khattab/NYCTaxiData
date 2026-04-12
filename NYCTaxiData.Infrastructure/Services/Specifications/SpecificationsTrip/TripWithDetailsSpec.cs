using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsTrip
{
    public class TripWithDetailsSpec : BaseSpecification<Trip>
    {
        public TripWithDetailsSpec(int tripId)
            : base(t => t.TripId == tripId)
        {
            AddInclude(t => t.Driver!);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);
            AddInclude(t => t.Simulation!);
        }
    }
}
