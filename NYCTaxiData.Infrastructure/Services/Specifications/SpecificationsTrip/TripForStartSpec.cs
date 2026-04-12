using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
{
    public class TripForStartSpec : BaseSpecification<Trip>
    {
        public TripForStartSpec(int tripId)
            : base(t => t.TripId == tripId && t.StartedAt == null)
        {
            AddInclude(t => t.Driver!);
        }
    }
    }
