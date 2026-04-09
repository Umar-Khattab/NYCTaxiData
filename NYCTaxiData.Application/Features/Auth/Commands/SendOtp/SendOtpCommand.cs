using MediatR; 
using NYCTaxiData.Domain.DTOs.Identity;

public record SendOtpCommand(string PhoneNumber) : IRequest<ResultDto>;