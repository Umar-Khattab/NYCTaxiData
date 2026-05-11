using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Managers
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
