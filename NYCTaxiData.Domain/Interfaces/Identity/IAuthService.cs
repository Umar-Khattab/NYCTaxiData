
using NYCTaxiData.Domain.DTOs.Identity;
using System.Threading.Tasks;
 
namespace NYCTaxiData.Domain.DTOs.Identity;

public interface IAuthService
{
    Task<UserResultDto> LoginAsync(LoginDto loginDto);
    Task<UserResultDto> RegisterDriverAsync(DriverRegisterDto driverregisterDto);

    Task<UserResultDto> RegisterManagerAsync(ManagerRegisterDto managerregisterDto);

    Task<ResultDto> SendOtpAsync(SendOtpDto dto);

    Task<VerifyOtpResultDto> VerifyOtpAsync(VerifyOtpDto dto);

    Task<UserResultDto> ResetPasswordAsync(ResetPasswordDto dto);

}