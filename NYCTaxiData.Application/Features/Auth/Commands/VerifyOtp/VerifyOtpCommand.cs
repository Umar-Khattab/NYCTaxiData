using MediatR;
 using NYCTaxiData.Application.DTOs.Identity;


public record VerifyOtpCommand( 
    string OtpCode
) : IRequest<VerifyOtpResultDto>;