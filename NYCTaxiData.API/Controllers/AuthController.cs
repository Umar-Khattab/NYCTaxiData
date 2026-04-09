using MediatR;
using Microsoft.AspNetCore.Mvc; 
using NYCTaxiData.Application.Auth.Commands.RegisterDriver;
using NYCTaxiData.Application.Auth.Commands.RegisterManager; 
using NYCTaxiData.Application.Auth.Commands.RefreshToken;
using NYCTaxiData.Application.Auth.Queries.GetProfile;
using NYCTaxiData.Application.Auth.Commands.Login;
using NYCTaxiData.Application.Features.Auth.Commands.ResetPassword; // تأكد من المسار ده

namespace NYCTaxiData.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator _mediator) : ControllerBase
{
    // POST api/v1/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return Unauthorized(new { message = "Invalid phone number or password" });

        return Ok(result);
    }


    // POST api/v1/auth/register/driver
    [HttpPost("register/driver")]
    public async Task<IActionResult> RegisterDriver([FromBody] RegisterDriverCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message ?? "Registration failed" });

        return CreatedAtAction(nameof(GetProfile),
            new { phoneNumber = result.FullName }, result);
    }


    // POST api/v1/auth/register/manager
    [HttpPost("register/manager")]
    public async Task<IActionResult> RegisterManager([FromBody] RegisterManagerCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message ?? "Registration failed" });

        return CreatedAtAction(nameof(GetProfile),
            new { phoneNumber = result.FullName }, result);
    }


    // POST api/v1/auth/otp/send
    [HttpPost("otp/send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }


    // POST api/v1/auth/otp/verify
    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = "Invalid or expired OTP" });

        return Ok(result);
    }


    // POST api/v1/auth/password/reset
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = "Invalid or expired reset token" });

        return Ok(result);
    }


    // POST api/v1/auth/token/refresh
    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return Unauthorized(new { message = "Token refresh failed" });

        return Ok(result);
    }


    // GET api/v1/auth/profile/{phoneNumber}
    [HttpGet("profile/{phoneNumber}")]
    public async Task<IActionResult> GetProfile([FromRoute] string phoneNumber)
    {
        var result = await _mediator.Send(new GetProfileQuery(phoneNumber));
        if (!result.IsSuccess)
            return NotFound(new { message = "User not found" });

        return Ok(result);
    }

}