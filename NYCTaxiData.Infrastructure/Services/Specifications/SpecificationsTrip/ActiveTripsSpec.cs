using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
{
    public class ActiveTripsSpec : BaseSpecification<Trip>
    {
        public ActiveTripsSpec()
            : base(t => t.StartedAt != null && t.EndedAt == null)
        {
            AddInclude(t => t.Driver!);
            AddOrderByDescending(t => t.StartedAt!);
        }
    }
}
