using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth
{
    public class UserForLoginSpec : BaseSpecification<User1>
    {
        public UserForLoginSpec(string phoneNumber)
            : base(u => u.Phonenumber == phoneNumber)
        {
            AddInclude(u => u.Driver!);
            AddInclude(u => u.Manager!);
        }
    }

}
