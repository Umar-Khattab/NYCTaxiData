using FluentValidation;

namespace NYCTaxiData.Application.Auth.Commands.RegisterDriver
{

    public class RegisterDriverCommandValidator : AbstractValidator<RegisterDriverCommand>
    {
        public RegisterDriverCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Matches(@"^\+?[0-9]{10,15}$");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.Age).InclusiveBetween(18, 70);
            RuleFor(x => x.LicenseNumber).NotEmpty();
            RuleFor(x => x.PlateNumber).NotEmpty();
        }
    }
}