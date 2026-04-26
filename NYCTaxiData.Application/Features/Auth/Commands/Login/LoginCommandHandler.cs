using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Auth.Commands.Login;
using NYCTaxiData.Application.Common.Specifications.Auth;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Application.Common.Interfaces.Services;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;

namespace NYCTaxiData.Application.Features.Auth.Commands.Login
{

	// LoginCommandHandler
 public class LoginCommandHandler(IUnitOfWork _uow, IJwtTokenService _jwt,IMapper mapper)
		: IRequestHandler<LoginCommand, UserResultDto>
	{
		public async Task<UserResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
		{
			var spec = new UserForLoginSpec(request.PhoneNumber);
			var user = await _uow.Users.GetBySpecAsync(spec);

			if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Passwordhash ?? ""))
				return new UserResultDto { IsSuccess = false, Message = "Invalid credentials" };

			var role = user.Driver != null ? "Driver"
					 : user.Manager != null ? "Manager"
					 : "User";
			var fullName = $"{user.Firstname} {user.Lastname}";

			// ✅ استخدم JwtTokenService
			var token = _jwt.GenerateToken(user.Phonenumber, role, fullName);
            var result = mapper.Map<UserResultDto>(user);

            // 3. ✅ تعبئة الحقول اللي مش موجودة في الـ User Entity يدويًا
            result.Token = token;
            result.IsSuccess = true;
            result.Role = role;
            result.FullName = fullName;

            return result;
        }
	}
}