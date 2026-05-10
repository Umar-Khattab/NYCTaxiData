using MediatR;
using NYCTaxiData.Application.DTOs.Identity; // 👈 السطر ده هو اللي ناقص
using NYCTaxiData.Application.Common.Plumping; // 👈 ستحتاجه أيضاً عشان كلاس الـ Result

namespace NYCTaxiData.Application.Auth.Commands.RegisterDriver
{
    // تأكد من تغيير النوع المرجع ليكون Result<UserResultDto> كما اتفقنا
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
    ) : IRequest<Result<UserResultDto>>;
}