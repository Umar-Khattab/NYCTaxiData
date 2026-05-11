using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
{
    public class TripHistoryCountSpec : BaseSpecification<Trip>
    {
        public TripHistoryCountSpec() { }

        public TripHistoryCountSpec(Guid driverId)
            : base(t => t.DriverId == driverId)
        {
        }
    }
}
