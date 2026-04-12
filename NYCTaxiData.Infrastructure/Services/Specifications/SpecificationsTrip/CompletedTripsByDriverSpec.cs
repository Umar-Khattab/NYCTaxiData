using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsTrip
{
    public class CompletedTripsByDriverSpec : BaseSpecification<Trip>
    {
        public CompletedTripsByDriverSpec(Guid driverId)
            : base(t => t.DriverId == driverId && t.EndedAt != null)
        {
            AddInclude(t => t.Driver!);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);
            AddOrderByDescending(t => t.EndedAt!);
        }
    }
}
