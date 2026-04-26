using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Common.Specifications.Auth;

public sealed class UserForResetPasswordSpec : BaseSpecification<User1>
{
    public UserForResetPasswordSpec(string phoneNumber)
        : base(u => u.Phonenumber == phoneNumber)
    {
    }
}
