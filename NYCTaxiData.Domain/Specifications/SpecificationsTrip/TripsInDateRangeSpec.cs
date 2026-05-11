using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
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
