using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth
{
    public class UserByPhoneSpec : BaseSpecification<User1>
    {
        public UserByPhoneSpec(string phoneNumber)
            : base(u => u.Phonenumber == phoneNumber)
        {
        }
    }
}
