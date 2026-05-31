using MediatR;
using NYCTaxiData.Application.DTOs.Identity; // ?? ÇáÓØÑ Ïå åæ Çááí äÇŞÕ
using NYCTaxiData.Application.Common.Plumbing; // ?? ÓÊÍÊÇÌå ÃíÖÇğ ÚÔÇä ßáÇÓ ÇáÜ Result

namespace NYCTaxiData.Application.Auth.Commands.RegisterDriver
{
    // ÊÃßÏ ãä ÊÛííÑ ÇáäæÚ ÇáãÑÌÚ áíßæä Result<UserResultDto> ßãÇ ÇÊİŞäÇ
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