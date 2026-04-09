using MediatR;
using NYCTaxiData.Domain.DTOs.Identity;
public class VerifyOtpCommandHandler(IAuthService _authService)
    : IRequestHandler<VerifyOtpCommand, VerifyOtpResultDto>
{
    public async Task<VerifyOtpResultDto> Handle(
        VerifyOtpCommand request, CancellationToken cancellationToken)
        => await _authService.VerifyOtpAsync(new VerifyOtpDto
        { 
            PhoneNumber = request.PhoneNumber, OtpCode = request.OtpCode 
        }
        );
}