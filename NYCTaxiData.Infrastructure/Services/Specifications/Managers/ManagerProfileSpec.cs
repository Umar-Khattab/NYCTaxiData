using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Managers
{
    public class ManagerProfileSpec : BaseSpecification<Manager>
    {
        public ManagerProfileSpec(Guid userId)
            : base(m => m.Id == userId)
        {
            AddInclude(m => m.IdNavigation!);
        }
    }
}
