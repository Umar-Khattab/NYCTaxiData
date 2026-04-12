using FluentValidation;

namespace NYCTaxiData.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.OldToken)
            .NotEmpty()
            .WithMessage("Token is required");
    }
}