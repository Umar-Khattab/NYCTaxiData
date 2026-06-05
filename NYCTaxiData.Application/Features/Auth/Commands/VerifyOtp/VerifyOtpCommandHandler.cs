using MediatR;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using System.Text;

public class VerifyOtpCommandHandler(
    IUnitOfWork _uow,
    ICacheService _cache,
    IJwtTokenService _jwt,
    ICurrentUserService _currentUser)  
    : IRequestHandler<VerifyOtpCommand, VerifyOtpResultDto>
{
    public async Task<VerifyOtpResultDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب المستخدم الحالي
        var user = await _uow.Users.GetByIdAsync(_currentUser.UserId);
        if (user == null) return new VerifyOtpResultDto { IsSuccess = false, Message = "Unauthorized" };

        var cacheKey = $"otp:{user.Id}";
        var cachedOtp = await _cache.GetAsync(cacheKey); // جلب القيمة من الكاش

        // 2. منطق التحقق (مع Bypass للتطوير)
        var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        bool isValid = (cachedOtp != null && cachedOtp.ToString() == request.OtpCode) || isDev;

        if (!isValid)
        {
            return new VerifyOtpResultDto { IsSuccess = false, Message = "Invalid OTP" };
        }

        // 3. التحقق نجح: مسح الكاش وتوليد التوكين
        await _cache.RemoveAsync(cacheKey);

        var resetToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.PhoneNumber}:{Guid.NewGuid()}:reset"));
        await _cache.SetAsync($"reset:{resetToken}", user.PhoneNumber, TimeSpan.FromMinutes(15));

        // ... (توليد الـ JWT Token كما فعلت سابقاً)
        return new VerifyOtpResultDto { IsSuccess = true, ResetToken = resetToken,Role= user.Role };
    }
}