using FluentValidation;

namespace NYCTaxiData.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Role).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty();
    }
}