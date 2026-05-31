using NYCTaxiData.Domain.Entities; 
using NYCTaxiData.Infrastructure.Services.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Specifications.Drivers
{
    public class DriverByIdSpec : BaseSpecification<Driver>
    {
        public DriverByIdSpec(Guid driverId)
            : base(d => d.UserId == driverId)
        {
            AddOrderBy(d => d.FullName!); 
            AddInclude(d => d.User!);
        }
    }
}
