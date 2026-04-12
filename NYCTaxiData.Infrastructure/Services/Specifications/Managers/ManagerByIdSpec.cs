using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Managers
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
