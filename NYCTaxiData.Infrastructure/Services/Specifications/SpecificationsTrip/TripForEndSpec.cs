using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
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
