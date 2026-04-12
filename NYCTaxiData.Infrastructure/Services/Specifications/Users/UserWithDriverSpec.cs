using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Users
{
    public class UserWithDriverSpec : BaseSpecification<User1>
    {
        public UserWithDriverSpec(Guid userId)
            : base(u => u.Id == userId)
        {
            AddInclude(u => u.Driver!);
        }
    }
}
