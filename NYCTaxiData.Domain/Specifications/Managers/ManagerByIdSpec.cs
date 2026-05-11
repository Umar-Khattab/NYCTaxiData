using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Managers
{
    public class ManagerByIdSpec : BaseSpecification<Manager>
    {
        public ManagerByIdSpec(Guid managerId)
            : base(m => m.Id == managerId)
        {
            AddInclude(m => m.IdNavigation!);
        }
    }
}
