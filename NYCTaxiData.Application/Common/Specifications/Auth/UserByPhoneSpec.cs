using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Common.Specifications.Auth;

public sealed class UserByPhoneSpec : BaseSpecification<User1>
{
    public UserByPhoneSpec(string phoneNumber)
        : base(u => u.Phonenumber == phoneNumber)
    {
    }
}
