
using MediatR;
using NYCTaxiData.Domain.DTOs.Identity;

namespace NYCTaxiData.Application.Auth.Commands.Login
{

    public record LoginCommand(string PhoneNumber, string Password) : IRequest<UserResultDto>
    {

    }
}