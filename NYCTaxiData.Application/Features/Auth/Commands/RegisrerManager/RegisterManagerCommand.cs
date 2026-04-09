using MediatR; 
using NYCTaxiData.Domain.DTOs.Identity;

namespace NYCTaxiData.Application.Auth.Commands.RegisterManager { 

public record RegisterManagerCommand(
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Password,
    int Age,
    string City,
    string Street,
    string EmployeeId,
    string Department
) : IRequest<UserResultDto>;
}