using FluentValidation;

public class SendOtpCommandValidator : AbstractValidator<SendOtpCommand>
{
    public SendOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{10,15}$");
    }
}