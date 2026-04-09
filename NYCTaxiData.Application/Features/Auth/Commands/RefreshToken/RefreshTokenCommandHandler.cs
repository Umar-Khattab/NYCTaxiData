using MediatR;
using NYCTaxiData.Domain.DTOs.Identity; // تأكد من الـ Namespace ده

namespace NYCTaxiData.Application.Auth.Commands.RefreshToken { 


    public class RefreshTokenCommandHandler(IAuthService _authService)
        : IRequestHandler<RefreshTokenCommand, UserResultDto>
    {
        public async Task<UserResultDto> Handle(
            RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await _authService.LoginAsync(new LoginDto
            {
                PhoneNumber = request.PhoneNumber
            });
        }
    }
}