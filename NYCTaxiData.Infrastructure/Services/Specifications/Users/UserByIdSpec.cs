using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth
{
    public class UserByIdSpec : BaseSpecification<User1>
    {
        public UserByIdSpec(Guid userId)
            : base(u => u.Id == userId)
        {
        }
    }
}
