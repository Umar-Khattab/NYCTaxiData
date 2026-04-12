using MediatR;
using NYCTaxiData.Application.DTOs.Identity; 

namespace NYCTaxiData.Application.Auth.Commands.RefreshToken
{

    public record RefreshTokenCommand(string OldToken) : IRequest<UserResultDto>;
}