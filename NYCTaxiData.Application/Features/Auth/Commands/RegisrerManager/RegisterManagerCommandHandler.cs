using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.Common.Specifications.Auth;
using NYCTaxiData.Application.Common.Specifications.Managers;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Auth.Commands.RegisterManager
{

 public class RegisterManagerCommandHandler(IUnitOfWork _uow, IMapper _mapper, IJwtTokenService _jwt)
	: IRequestHandler<RegisterManagerCommand, UserResultDto>
	{
		public async Task<UserResultDto> Handle(RegisterManagerCommand request, CancellationToken cancellationToken)
		{
			if (await _uow.Users.AnyWithSpecAsync(new UserPhoneExistsSpec(request.PhoneNumber), cancellationToken))
				return new UserResultDto { IsSuccess = false, Message = "Phone number already exists" };

			if (await _uow.Managers.AnyWithSpecAsync(new ManagerEmployeeIdExistsSpec(request.EmployeeId), cancellationToken))
				return new UserResultDto { IsSuccess = false, Message = "Employee ID already exists" };

			User1 user = _mapper.Map<User1>(request);
			var manager = _mapper.Map<Manager>(request);

			manager.Id = user.Id;

			await _uow.Users.AddAsync(user);
			await _uow.Managers.AddAsync(manager);
			await _uow.SaveChangesAsync(cancellationToken);

			var fullName = $"{user.Firstname} {user.Lastname}";
			var token = _jwt.GenerateToken(user.Phonenumber, "Manager", fullName);

			return new UserResultDto
			{
				IsSuccess = true,
				FullName = fullName,
				Role = "Manager",
				Token = token
			};
		}
	}
}