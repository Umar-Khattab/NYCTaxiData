using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Drivers
{
    public class DriverPlateExistsSpec : BaseSpecification<Driver> 
    {
        public DriverPlateExistsSpec(string plate)
            : base(d => d.PlateNumber == plate)
        {
        }
    }
}
