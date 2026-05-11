using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Users
{
    public class UserForResetPasswordSpec : BaseSpecification<User1>
    {
        public UserForResetPasswordSpec(string phoneNumber)
            : base(u => u.PhoneNumber == phoneNumber)
        {
        }
    }
}
