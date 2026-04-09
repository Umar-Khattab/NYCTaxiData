using MediatR;
using NYCTaxiData.Domain.DTOs.Identity;

namespace NYCTaxiData.Application.Auth.Commands.RegisterDriver
{

    public record RegisterDriverCommand(
        string FirstName,
        string LastName,
        string PhoneNumber,
        string Password,
        int Age,
        string City,
        string Street,
        string LicenseNumber,
        string PlateNumber
    ) : IRequest<UserResultDto>;
}