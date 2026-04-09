using MediatR;
using NYCTaxiData.Application.Auth.Commands.Login;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Domain.DTOs.Identity;  

namespace NYCTaxiData.Application.Features.Auth.Commands.Login
{

    public class LoginCommandHandler(IAuthService _authService)
        : IRequestHandler<LoginCommand, UserResultDto>
    {
        public async Task<UserResultDto> Handle(
            LoginCommand request, CancellationToken cancellationToken)
        {
            return await _authService.LoginAsync(new LoginDto
            {
                PhoneNumber = request.PhoneNumber,
                Password = request.Password
            });
        }
    }
}