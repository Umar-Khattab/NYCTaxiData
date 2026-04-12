using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text; // تأكد من الـ Namespace ده

namespace NYCTaxiData.Application.Auth.Commands.RefreshToken {


	// RefreshTokenCommandHandler
	public class RefreshTokenCommandHandler(IUnitOfWork _uow, JwtTokenService _jwt, IMapper mapper)
		: IRequestHandler<RefreshTokenCommand, UserResultDto>
	{
		public async Task<UserResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
		{
			// التحقق من الـ Token القديم وجيب الـ Phone
			var tokenHandler = new JwtSecurityTokenHandler();
			JwtSecurityToken? jwtToken;
			try
			{
				jwtToken = tokenHandler.ReadJwtToken(request.OldToken);
			}
			catch
			{
				return new UserResultDto { IsSuccess = false, Message = "Invalid token" };
			}

			var phoneNumber = jwtToken.Claims
				.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(phoneNumber))
				return new UserResultDto { IsSuccess = false, Message = "Invalid token claims" };

			var spec = new UserForLoginSpec(phoneNumber);
			var user = await _uow.Users.GetBySpecAsync(spec);
			if (user == null)
				return new UserResultDto { IsSuccess = false, Message = "User not found" };

			var role = user.Driver != null ? "Driver"
					 : user.Manager != null ? "Manager"
					 : "User";
			var fullName = $"{user.Firstname} {user.Lastname}";

			// ✅ استخدم JwtTokenService
			var newToken = _jwt.GenerateToken(phoneNumber, role, fullName);

			var result = mapper.Map<UserResultDto>(user);
			return result;
		}
	}
}