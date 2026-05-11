using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
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
