using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Features.Auth.Commands.VerifyOtp
{
    public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
    {
        public VerifyOtpCommandValidator()
        {

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required")
                .Length(6).WithMessage("OTP must be 6 digits")
                .Matches(@"^\d{6}$").WithMessage("OTP must contain digits only");
        }
    }
}
