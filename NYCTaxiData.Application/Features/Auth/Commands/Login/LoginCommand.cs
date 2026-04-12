
using MediatR;
using NYCTaxiData.Application.DTOs.Identity;

namespace NYCTaxiData.Application.Auth.Commands.Login
{

    public record LoginCommand(string PhoneNumber, string Password) : IRequest<UserResultDto>
    {

    }
}