using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Identity; 
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Features.Auth.Commands.ResetPassword
{
	public class ResetPasswordCommandHandler(IUnitOfWork _uow, ICacheService _cache, IMapper _mapper)
	 : IRequestHandler<ResetPasswordCommand, UserResultDto>
	{
		public async Task<UserResultDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
		{
			var phoneKey = await _cache.GetAsync($"reset:{request.ResetToken}");
			if (string.IsNullOrEmpty(phoneKey))
				return new UserResultDto { IsSuccess = false, Message = "Invalid or expired reset token" };

			// ✅ استخدام الـ Specification لجلب المستخدم
			var spec = new UserForResetPasswordSpec(phoneKey);
			var user = await _uow.Users.GetBySpecAsync(spec);

			if (user == null)
				return new UserResultDto { IsSuccess = false, Message = "User not found" };

			// عمل Hash لكلمة المرور الجديدة
			user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

			await _uow.Users.UpdateAsync(user);
			await _uow.SaveChangesAsync(cancellationToken);

			// مسح توكن الاستعادة من الـ Cache بعد الاستخدام
			await _cache.RemoveAsync($"reset:{request.ResetToken}");

			var result = _mapper.Map<UserResultDto>(user);
			result.IsSuccess = true;
			result.Message = "Password reset successfully";

			return result;
		}
	}
}