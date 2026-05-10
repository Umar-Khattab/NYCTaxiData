using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.ConcurrentEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Drivers
{
    public class DriverByIdSpec : BaseSpecification<Driver>
    {
        public DriverByIdSpec(Guid driverId)
            : base(d => d.UserId == driverId)
        {
        }
    }
}
