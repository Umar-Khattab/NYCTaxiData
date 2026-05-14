using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
{
    public class TripWithDetailsSpec : BaseSpecification<Trip>
    {
        public TripWithDetailsSpec(int tripId)
            : base(t => t.TripId == tripId)
        {
            AddInclude(t => t.Driver!);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!); 
        }
    }
}
