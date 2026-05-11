using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
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
