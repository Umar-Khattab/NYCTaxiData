using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Managers
{
    public class ManagerByEmployeeIdSpec : BaseSpecification<Manager>
    {
        public ManagerByEmployeeIdSpec(string employeeId)
            : base(m => m.Employeeid == employeeId)
        {
            AddInclude(m => m.IdNavigation!);
        }
    }
}
