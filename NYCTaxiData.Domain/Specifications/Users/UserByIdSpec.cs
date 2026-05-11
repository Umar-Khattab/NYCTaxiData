using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure.Services.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Specifications.Users
{
    public class UserByIdSpec : BaseSpecification<User1>
    {
        public UserByIdSpec(Guid userId)
            : base(u => u.Id == userId)
        {
        }
    }
}
