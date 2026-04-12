using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Auth.Commands.RegisterDriver;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure; // ده مهم عشان يشوف User1
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;

namespace NYCTaxiData.Application.Auth.Commands.RegisterDriver
{
	public class RegisterDriverCommandHandler(IUnitOfWork _uow, IMapper _mapper, JwtTokenService _jwt)
	  : IRequestHandler<RegisterDriverCommand, UserResultDto>
	{
		public async Task<UserResultDto> Handle(RegisterDriverCommand request, CancellationToken cancellationToken)
		{
			if (await _uow.Users.AnyWithSpecAsync(new UserPhoneExistsSpec(request.PhoneNumber), cancellationToken))
				return new UserResultDto { IsSuccess = false, Message = "Phone number already exists" };

			User1 user = _mapper.Map<User1>(request);
			var driver = _mapper.Map<Driver>(request);

			driver.Id = user.Id;

			await _uow.Users.AddAsync(user);
			await _uow.Drivers.AddAsync(driver);
			await _uow.SaveChangesAsync(cancellationToken);

			var fullName = $"{user.Firstname} {user.Lastname}";
			// ✅ توليد التوكن فوراً بعد التسجيل
			var token = _jwt.GenerateToken(user.Phonenumber, "Driver", fullName);

			return new UserResultDto
			{
				IsSuccess = true,
				FullName = fullName,
				Role = "Driver",
				Token = token // تأكد أن UserResultDto فيه Property اسمها Token
			};
		}
	}
}