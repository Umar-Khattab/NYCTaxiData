using MediatR;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Controllers.Base;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.Auth.Commands.Login;
using NYCTaxiData.Application.Auth.Commands.RegisterDriver;
using NYCTaxiData.Application.Auth.Commands.RegisterManager;
using NYCTaxiData.Application.Auth.Commands.RefreshToken;
using NYCTaxiData.Application.Auth.Queries.GetProfile;
using NYCTaxiData.Application.Features.Auth.Commands.ResetPassword;
using NYCTaxiData.Application.Features.Auth.Commands.SendOtp; // تأكد من الـ Namespace ده
using NYCTaxiData.Application.Features.Auth.Commands.VerifyOtp;
using Twilio.Types; // تأكد من الـ Namespace ده

namespace NYCTaxiData.API.Controllers;

public class AuthController : BaseController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        // استخدم (dynamic) عشان الـ BaseController يقبل أي Result<T>
        return HandleResult((dynamic)await Mediator.Send(command));
    }

    // 2. Register Driver
    [HttpPost("register/driver")]
    public async Task<IActionResult> RegisterDriver([FromBody] RegisterDriverCommand command)
    {
        var result = await Mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Message);
    }

    // 3. Register Manager
    [HttpPost("register/manager")]
    public async Task<IActionResult> RegisterManager([FromBody] RegisterManagerCommand command)
    {
        return HandleResult((dynamic)await Mediator.Send(command));
    }

    // 4. Send OTP
    [HttpPost("otp/send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpCommand command)
    {
        var result = await Mediator.Send(command);

        if (result.IsSuccess)
        { 
            return OkResult(new { }, result.Message ?? "OTP sent successfully");
        } 
        return BadRequestResult(result.Message ?? "Failed to send OTP");
    }

    // 5. Verify OTP
    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        var result = await Mediator.Send(command);

        if (result.IsSuccess)
        {
            // بنرجع الداتا اللي راجعة في الـ DTO (زي التوكن مثلاً)
            return OkResult(result, result.Message ?? "OTP verified successfully");
        }

        // لو فشل نرجع BadRequest بالرسالة اللي جاية من الـ Handler
        return BadRequestResult(result.Message ?? "Invalid OTP");
    }

    // 6. Reset Password
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await Mediator.Send(command);

        // 2. بما إن الـ Reset Password غالباً بيرجع Success/Failure بس
        // بنفحص الـ IsSuccess بتاعة الـ DTO اللي راجع
        if (result.IsSuccess)
        {
            return Ok(result); // أو OkResult لو الميثود موجودة في الـ Base
        }

        return BadRequest(result);
    }

    // 7. Refresh Token
    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        return HandleResult((dynamic)await Mediator.Send(command));
    }

    // 8. Get Profile
    [HttpGet("profile/{phoneNumber}")]
    public async Task<IActionResult> GetProfile([FromRoute] string phoneNumber)
    {
        var result = await Mediator.Send(new GetProfileQuery(phoneNumber));

        // لو النتيجة مش null يبقى نجاح، غير كدة نرجع 404
        if (result != null)
        {
            return Ok(new { isSuccess = true, data = result });
        }

        return NotFound(new { isSuccess = false, message = "User not found" });
    }
}