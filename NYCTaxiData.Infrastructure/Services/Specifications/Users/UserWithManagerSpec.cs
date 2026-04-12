using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Users
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
