using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth
{
    public class UserForResetPasswordSpec : BaseSpecification<User1>
    {
        public UserForResetPasswordSpec(string phoneNumber)
            : base(u => u.Phonenumber == phoneNumber)
        {
        }
    }
}
