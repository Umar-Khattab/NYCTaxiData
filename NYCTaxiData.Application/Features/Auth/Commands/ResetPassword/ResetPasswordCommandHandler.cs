using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Identity; 
using NYCTaxiData.Domain.DTOs.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler(IAuthService _authService)
       : IRequestHandler<ResetPasswordCommand, UserResultDto>
    {
        public async Task<UserResultDto> Handle(
            ResetPasswordCommand request, CancellationToken cancellationToken)
            => await _authService.ResetPasswordAsync(new ResetPasswordDto
            { 
                ResetToken = request.ResetToken, NewPassword = request.NewPassword 
            }
            );
    }
}
