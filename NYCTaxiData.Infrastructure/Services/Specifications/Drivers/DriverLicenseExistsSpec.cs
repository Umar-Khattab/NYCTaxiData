using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Drivers
{
    public class DriverLicenseExistsSpec : BaseSpecification<Driver>
    {
        public DriverLicenseExistsSpec(string license)
            : base(d => d.LicenseNumber == license)
        {
        }
    }
}
