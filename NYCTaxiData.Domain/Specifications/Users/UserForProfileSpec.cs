using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Users
{
    public class UserForProfileSpec : BaseSpecification<User1>
    {
        // بالـ Phone
        public UserForProfileSpec(string phoneNumber)
            : base(u => u.PhoneNumber == phoneNumber)
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
