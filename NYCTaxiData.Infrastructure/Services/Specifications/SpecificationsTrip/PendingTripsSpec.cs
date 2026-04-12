using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsTrip
{
    public class PendingTripsSpec : BaseSpecification<Trip>
    {
        public PendingTripsSpec()
            : base(t => t.StartedAt == null && t.EndedAt == null)
        {
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);
            AddOrderBy(t => t.TripId);
        }
    }
}
