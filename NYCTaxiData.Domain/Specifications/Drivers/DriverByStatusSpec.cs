using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Infrastructure.Services.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Specifications.Drivers
{
    public class DriverByStatusSpec : BaseSpecification<Driver>
    {
        public DriverByStatusSpec(CurrentStatus status)
            : base(d => d.Status == status)
        {
            AddOrderBy(d => d.FullName!);
        }
    }
}
