using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsTrip
{
    public class TripsInDateRangeSpec : BaseSpecification<Trip>
    {
        public TripsInDateRangeSpec(DateTime from, DateTime to)
            : base(t => t.StartedAt >= from && t.StartedAt <= to)
        {
            AddInclude(t => t.Driver!);
            AddOrderByDescending(t => t.StartedAt!);
        }
    }
}
