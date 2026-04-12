using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth
{
    public class UserForProfileSpec : BaseSpecification<User1>
    {
        // بالـ Phone
        public UserForProfileSpec(string phoneNumber)
            : base(u => u.Phonenumber == phoneNumber)
        {
            AddInclude(u => u.Driver!);
            AddInclude(u => u.Manager!);
        }

        // بالـ Id
        public UserForProfileSpec(Guid userId)
            : base(u => u.Id == userId)
        {
            AddInclude(u => u.Driver!);
            AddInclude(u => u.Manager!);
        }
    }
}
