using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
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
