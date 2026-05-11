using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
{
    public class TripHistorySpec : BaseSpecification<Trip>
    { 
        public TripHistorySpec(Guid? driverId, int page, int limit)
            : base(t => !driverId.HasValue || t.DriverId == driverId.Value)
        { 
            AddInclude(t => t.Driver!);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);

            AddOrderByDescending(t => t.StartedAt!);  
            ApplyPaging((page - 1) * limit, limit);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);

        }
    }
}
