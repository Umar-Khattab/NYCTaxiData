using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Users
{
    public class UserWithRolesSpec : BaseSpecification<User1>
    {
        public UserWithRolesSpec(string phoneNumber)
            : base(u => u.Phonenumber == phoneNumber)
        {
            AddInclude(u => u.Driver!);
            AddInclude(u => u.Manager!);
        }
    }
}
