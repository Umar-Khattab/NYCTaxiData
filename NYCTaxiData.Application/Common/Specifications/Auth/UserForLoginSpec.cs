using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Common.Specifications.Auth;

public sealed class UserForLoginSpec : BaseSpecification<User1>
{
    public UserForLoginSpec(string phoneNumber)
        : base(u => u.Phonenumber == phoneNumber)
    {
        AddInclude(u => u.Driver!);
        AddInclude(u => u.Manager!);
    }
}
