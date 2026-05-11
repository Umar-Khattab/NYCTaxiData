using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
{
    public class TripForEndSpec : BaseSpecification<Trip>
    {
        public TripForEndSpec(int tripId)
            : base(t => t.TripId == tripId &&
                        t.StartedAt != null &&
                        t.EndedAt == null)
        {
            AddInclude(t => t.Driver!);
        }
    }
}
