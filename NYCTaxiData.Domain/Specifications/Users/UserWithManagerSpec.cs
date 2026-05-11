using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Users
{
    public class UserWithManagerSpec : BaseSpecification<User1>
    {
        public UserWithManagerSpec(Guid userId)
            : base(u => u.Id == userId)
        {
            AddInclude(u => u.Manager!);
        }
    }
}
