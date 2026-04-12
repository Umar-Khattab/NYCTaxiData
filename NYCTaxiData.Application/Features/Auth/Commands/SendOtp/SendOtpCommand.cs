using MediatR; 
using NYCTaxiData.Application.DTOs.Identity;

public record SendOtpCommand(string PhoneNumber) : IRequest<ResultDto>;