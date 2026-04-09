using MediatR;
 using NYCTaxiData.Domain.DTOs.Identity;

public record VerifyOtpCommand(string PhoneNumber, string OtpCode)
    : IRequest<VerifyOtpResultDto>;