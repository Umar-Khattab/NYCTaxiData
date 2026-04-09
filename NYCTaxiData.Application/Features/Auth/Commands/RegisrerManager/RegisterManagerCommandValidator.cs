using FluentValidation;

namespace NYCTaxiData.Application.Auth.Commands.RegisterManager { 
public class RegisterManagerCommandValidator : AbstractValidator<RegisterManagerCommand>
{
    public RegisterManagerCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{10,15}$");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Department).NotEmpty().MaximumLength(100);
    }
}
}