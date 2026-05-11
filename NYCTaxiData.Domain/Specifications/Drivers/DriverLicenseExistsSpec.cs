using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;

namespace NYCTaxiData.Domain.Specifications.Drivers
{
    public class DriverLicenseExistsSpec : BaseSpecification<Driver>
    {
        public DriverLicenseExistsSpec(string license)
            : base(d => d.LicenseNumber == license)
        {
        }
    }
}
