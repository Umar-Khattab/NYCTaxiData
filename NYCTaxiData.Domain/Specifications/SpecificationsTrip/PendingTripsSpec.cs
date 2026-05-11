using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
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
