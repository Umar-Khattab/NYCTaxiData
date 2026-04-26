using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Common.Specifications.Auth;

public sealed class UserPhoneExistsSpec : BaseSpecification<User1>
{
    public UserPhoneExistsSpec(string phoneNumber)
        : base(u => u.Phonenumber == phoneNumber)
    {
    }
}
