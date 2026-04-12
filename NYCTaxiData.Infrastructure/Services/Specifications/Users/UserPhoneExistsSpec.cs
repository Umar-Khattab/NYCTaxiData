using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth
{
    public class UserPhoneExistsSpec : BaseSpecification<User1>
    {
        public UserPhoneExistsSpec(string phoneNumber)
            : base(u => u.Phonenumber == phoneNumber)
        {
        }
    }
}
