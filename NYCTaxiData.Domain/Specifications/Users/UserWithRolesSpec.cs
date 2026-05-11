using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Users
{
    public class UserWithRolesSpec : BaseSpecification<User1>
    {
        public UserWithRolesSpec(string phoneNumber)
            : base(u => u.PhoneNumber == phoneNumber)
        {
            AddInclude(u => u.Driver!);
            AddInclude(u => u.Manager!);
        }
    }
}
