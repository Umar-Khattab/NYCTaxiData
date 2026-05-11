using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
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
