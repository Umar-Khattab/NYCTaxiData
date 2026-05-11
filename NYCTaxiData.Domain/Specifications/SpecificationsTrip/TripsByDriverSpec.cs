using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
{
    public class TripsByDriverSpec : BaseSpecification<Trip>
    {
        public TripsByDriverSpec(Guid driverId)
            : base(t => t.DriverId == driverId)
        {
            AddInclude(t => t.Driver!);
            AddOrderByDescending(t => t.StartedAt!);
        }
    }
}
