using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Drivers
{
    public class DriverByStatusSpec : BaseSpecification<Driver>
    {
        public DriverByStatusSpec(CurrentStatus status)
            : base(d => d.Status == status)
        {
            AddOrderBy(d => d.Fullname!);
        }
    }
}
