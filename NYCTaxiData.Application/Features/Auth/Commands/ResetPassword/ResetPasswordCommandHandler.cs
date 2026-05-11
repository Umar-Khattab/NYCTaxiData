using AutoMapper;
using MediatR; 
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Domain.Specifications.Users;

namespace NYCTaxiData.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler(IUnitOfWork _uow, ICacheService _cache, IMapper _mapper)
     : IRequestHandler<ResetPasswordCommand, UserResultDto>
    {
        public async Task<UserResultDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var phoneKeyRaw = await _cache.GetAsync($"reset:{request.ResetToken}");
            var phoneKey = phoneKeyRaw?.ToString();

            // 2. 🔍 سطر الـ Debug اللي هيعرفنا التوكن موجود ولا لا
            Console.WriteLine($"🔍 DEBUG RESET: Key=reset:{request.ResetToken} | FoundPhone={phoneKey}"); if (string.IsNullOrEmpty(phoneKey))
            {
                return new UserResultDto { IsSuccess = false, Message = "Invalid or expired reset token" };
            }
             
            var spec = new UserForResetPasswordSpec(phoneKey);
            var user = await _uow.Users.GetBySpecAsync(spec);

            if (user == null)
            {
                return new UserResultDto { IsSuccess = false, Message = "User not found" };
            }
             
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            await _uow.Users.UpdateAsync(user);
            await _uow.SaveChangesAsync(cancellationToken);
             
            await _cache.RemoveAsync($"reset:{request.ResetToken}");
             
            var result = _mapper.Map<UserResultDto>(user);
            result.IsSuccess = true;
            result.Message = "Password reset successfully";

            return result;
        }
    }
}