using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Specifications.Managers
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
