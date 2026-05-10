using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Infrastructure.Services.ConcurrentEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Drivers
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
