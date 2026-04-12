using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
// VerifyOtpCommandHandler
public class VerifyOtpCommandHandler(IUnitOfWork _uow, ICacheService _cache, JwtTokenService _jwt,IMapper mapper)
	: IRequestHandler<VerifyOtpCommand, VerifyOtpResultDto>
{
	public async Task<VerifyOtpResultDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
	{
		var cachedOtp = await _cache.GetAsync($"otp:{request.PhoneNumber}");
		if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != request.OtpCode)
			return new VerifyOtpResultDto { IsSuccess = false, Message = "Invalid or expired OTP" };

		await _cache.RemoveAsync($"otp:{request.PhoneNumber}");

		var resetToken = Convert.ToBase64String(
			Encoding.UTF8.GetBytes($"{request.PhoneNumber}:{Guid.NewGuid()}:reset"));
		await _cache.SetAsync($"reset:{resetToken}", request.PhoneNumber, TimeSpan.FromMinutes(15));

		var spec = new UserForLoginSpec(request.PhoneNumber);
		var user = await _uow.Users.GetBySpecAsync(spec);

		if (user == null)
			return new VerifyOtpResultDto { IsSuccess = true, ResetToken = resetToken };

		var role = user.Driver != null ? "Driver"
				 : user.Manager != null ? "Manager"
				 : "User";
		var fullName = $"{user.Firstname} {user.Lastname}";

		// ✅ استخدم JwtTokenService
		var token = _jwt.GenerateToken(user.Phonenumber, role, fullName);

		var result = mapper.Map<VerifyOtpResultDto>(user);
		return result;
	}
}