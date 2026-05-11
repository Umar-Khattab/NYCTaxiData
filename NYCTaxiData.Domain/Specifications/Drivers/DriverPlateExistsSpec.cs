using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Drivers
{
    public class DriverPlateExistsSpec : BaseSpecification<Driver> 
    {
        public DriverPlateExistsSpec(string plate)
            : base(d => d.PlateNumber == plate)
        {
        }
    }
}
