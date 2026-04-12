using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Users
{
    public class UserByEmailSpec : BaseSpecification<User1>
    {
        public UserByEmailSpec(string email)
            : base(u => u.Email == email)
        {
        }
    }
}
