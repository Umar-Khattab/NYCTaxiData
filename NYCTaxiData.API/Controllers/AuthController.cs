using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Controllers.Base;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.Auth.Commands.Login;
using NYCTaxiData.Application.Auth.Commands.RegisterDriver;
using NYCTaxiData.Application.Auth.Commands.RegisterManager;
using NYCTaxiData.Application.Auth.Commands.RefreshToken;
using NYCTaxiData.Application.Auth.Queries.GetProfile;
using NYCTaxiData.Application.Features.Auth.Commands.ResetPassword; 

namespace NYCTaxiData.API.Controllers;

public class AuthController : BaseController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var data = await Mediator.Send(command);
        var result = Result.Success(data); // لف الداتا يدوياً
        return HandleResult(result);
    }

    [HttpPost("register/driver")]
    public async Task<IActionResult> RegisterDriver([FromBody] RegisterDriverCommand command)
    {
        var data = await Mediator.Send(command);
        var result = Result.Success(data);
        return HandleResult(result);
    }

    [HttpPost("register/manager")]
    public async Task<IActionResult> RegisterManager([FromBody] RegisterManagerCommand command)
    {
        var data = await Mediator.Send(command);
        var result = Result.Success(data);
        return HandleResult(result);
    }

    [HttpPost("otp/send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpCommand command)
    {
        var data = await Mediator.Send(command);
        var result = Result.Success(data);
        return HandleResult(result);
    }

    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        var data = await Mediator.Send(command);
        var result = Result.Success(data);
        return HandleResult(result);
    }

    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var data = await Mediator.Send(command);
        var result = Result.Success(data);
        return HandleResult(result);
    }

    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var data = await Mediator.Send(command);
        var result = Result.Success(data);
        return HandleResult(result);
    }

    [HttpGet("profile/{phoneNumber}")]
    public async Task<IActionResult> GetProfile([FromRoute] string phoneNumber)
    {
        var data = await Mediator.Send(new GetProfileQuery(phoneNumber));
        var result = Result.Success(data);
        return HandleResult(result);
    }
}