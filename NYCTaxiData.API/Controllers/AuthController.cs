using MediatR;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.Application.Auth.Commands.RegisterDriver;
using NYCTaxiData.Application.Auth.Commands.RegisterManager;
using NYCTaxiData.Application.Auth.Commands.RefreshToken;
using NYCTaxiData.Application.Auth.Queries.GetProfile;
using NYCTaxiData.Application.Auth.Commands.Login;
using NYCTaxiData.Application.Features.Auth.Commands.ResetPassword;

namespace NYCTaxiData.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator _mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return Unauthorized(new { message = "Invalid phone number or password" });
        return Ok(result);
    }

    [HttpPost("register/driver")]
    public async Task<IActionResult> RegisterDriver([FromBody] RegisterDriverCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message ?? "Registration failed" });
        return CreatedAtAction(nameof(GetProfile), new { phoneNumber = result.FullName }, result);
    }

    [HttpPost("register/manager")]
    public async Task<IActionResult> RegisterManager([FromBody] RegisterManagerCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message ?? "Registration failed" });
        return CreatedAtAction(nameof(GetProfile), new { phoneNumber = result.FullName }, result);
    }

    [HttpPost("otp/send")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });
        return Ok(new { message = result.Message });
    }

    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });
        return Ok(result);
    }

    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });
        return Ok(result);
    }

    [HttpPost("token/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return Unauthorized(new { message = result.Message });
        return Ok(result);
    }

    [HttpGet("profile/{phoneNumber}")]
    public async Task<IActionResult> GetProfile([FromRoute] string phoneNumber)
    {
        var result = await _mediator.Send(new GetProfileQuery(phoneNumber));
        if (!result.IsSuccess)
            return NotFound(new { message = "User not found" });
        return Ok(result);
    }
}