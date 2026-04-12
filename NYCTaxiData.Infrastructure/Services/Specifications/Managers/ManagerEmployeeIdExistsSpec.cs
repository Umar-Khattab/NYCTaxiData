using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Managers
{
    public class ManagerEmployeeIdExistsSpec : BaseSpecification<Manager>
    {
        public ManagerEmployeeIdExistsSpec(string employeeId)
            : base(m => m.Employeeid == employeeId)
        {
        }
    }
}
