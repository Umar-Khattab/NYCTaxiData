using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Infrastructure.Services.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Specifications.Drivers
{
    public class AvailableDriversSpec : BaseSpecification<Driver>
    {
        public AvailableDriversSpec()
            : base(d => d.Status == CurrentStatus.Available)
        {
            AddOrderBy(d => d.FullName!);
        }
    }
}
