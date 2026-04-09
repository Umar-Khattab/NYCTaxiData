using MediatR;
using NYCTaxiData.Domain.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Features.Auth.Commands.ResetPassword
{
    public record ResetPasswordCommand(string ResetToken, string NewPassword)
      : IRequest<UserResultDto>;
}
