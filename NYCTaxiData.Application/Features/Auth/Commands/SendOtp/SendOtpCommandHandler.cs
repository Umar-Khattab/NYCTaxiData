using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Domain.DTOs.Identity;

public class SendOtpCommandHandler(IAuthService _authService)
    : IRequestHandler<SendOtpCommand, ResultDto>
{
    public async Task<ResultDto> Handle(
        SendOtpCommand request, CancellationToken cancellationToken)
        => await _authService.SendOtpAsync(new SendOtpDto
        { 
            PhoneNumber = request.PhoneNumber
        }
        );
}