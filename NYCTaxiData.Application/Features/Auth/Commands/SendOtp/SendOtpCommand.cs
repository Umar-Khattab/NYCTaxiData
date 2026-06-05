using MediatR; 
using NYCTaxiData.Application.DTOs.Identity;

public record SendOtpCommand() : IRequest<ResultDto>;