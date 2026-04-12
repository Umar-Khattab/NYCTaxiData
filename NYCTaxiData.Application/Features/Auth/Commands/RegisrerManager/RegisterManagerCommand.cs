using MediatR; 
using NYCTaxiData.Application.DTOs.Identity;

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