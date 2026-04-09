using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Identity; 
using NYCTaxiData.Domain.DTOs.Identity;

namespace NYCTaxiData.Application.Auth.Commands.RegisterDriver
{

    public class RegisterDriverCommandHandler(IAuthService _authService)
        : IRequestHandler<RegisterDriverCommand, UserResultDto>
    {
        public async Task<UserResultDto> Handle(
            RegisterDriverCommand request, CancellationToken cancellationToken)
        {
            return await _authService.RegisterDriverAsync(new DriverRegisterDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                Age = request.Age,
                City = request.City,
                Street = request.Street,
                LicenseNumber = request.LicenseNumber,
                PlateNumber = request.PlateNumber
            });
        }
    }
}