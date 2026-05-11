using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Managers
{
    public class ManagerEmployeeIdExistsSpec : BaseSpecification<Manager>
    {
        public ManagerEmployeeIdExistsSpec(string employeeId)
            : base(m => m.Employeeid == employeeId)
        {
        }
    }
}
