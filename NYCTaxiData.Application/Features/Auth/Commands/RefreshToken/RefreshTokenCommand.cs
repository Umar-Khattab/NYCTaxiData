using MediatR;
using NYCTaxiData.Domain.DTOs.Identity; 

namespace NYCTaxiData.Application.Auth.Commands.RefreshToken
{

    public record RefreshTokenCommand(string PhoneNumber, string Role, string FullName)
        : IRequest<UserResultDto>;
}