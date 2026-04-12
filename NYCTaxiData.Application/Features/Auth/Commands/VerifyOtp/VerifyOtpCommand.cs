using MediatR;
 using NYCTaxiData.Application.DTOs.Identity;


public record VerifyOtpCommand(
    string PhoneNumber,
    string OtpCode
) : IRequest<VerifyOtpResultDto>;