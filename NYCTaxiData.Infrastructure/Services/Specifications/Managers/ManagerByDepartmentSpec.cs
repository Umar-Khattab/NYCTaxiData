using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Managers
{
    public class ManagerByDepartmentSpec : BaseSpecification<Manager>
    {
        public ManagerByDepartmentSpec(string department)
            : base(m => m.Department == department)
        {
            AddInclude(m => m.IdNavigation!);
            AddOrderBy(m => m.Employeeid);
        }
    }
}
