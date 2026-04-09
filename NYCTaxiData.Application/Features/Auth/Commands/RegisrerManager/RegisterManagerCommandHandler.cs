using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Domain.DTOs.Identity;

namespace NYCTaxiData.Application.Auth.Commands.RegisterManager
{

    public class RegisterManagerCommandHandler(IAuthService _authService)
        : IRequestHandler<RegisterManagerCommand, UserResultDto>
    {
        public async Task<UserResultDto> Handle(
            RegisterManagerCommand request, CancellationToken cancellationToken)
        {
            return await _authService.RegisterManagerAsync(new ManagerRegisterDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                Age = request.Age,
                City = request.City,
                Street = request.Street,
                EmployeeId = request.EmployeeId,
                Department = request.Department
            });
        }
    }
}