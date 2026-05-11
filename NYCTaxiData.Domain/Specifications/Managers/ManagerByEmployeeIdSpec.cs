using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Managers
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
