using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;
using StackExchange.Redis;
using System.Text;

public class VerifyOtpCommandHandler(
    IUnitOfWork _uow,
    ICacheService _cache,
    JwtTokenService _jwt,
    IMapper _mapper)
    : IRequestHandler<VerifyOtpCommand, VerifyOtpResultDto> // تأكد إن الـ DTO صح هنا
{
    public async Task<VerifyOtpResultDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        // 1. تنظيف الرقم
        var cleanPhone = request.PhoneNumber.Trim().Replace(" ", "");
        if (cleanPhone.StartsWith("20")) cleanPhone = "0" + cleanPhone.Substring(2);

        var cacheKey = $"otp:{cleanPhone}";

        // 2. قراءة الكاش
        var cachedOtpRaw = await _cache.GetAsync(cacheKey);
        var cachedOtp = cachedOtpRaw?.ToString();

        Console.WriteLine($"🔍 DEBUG: Key={cacheKey} | CachedValue='{cachedOtp}' | UserSent='{request.OtpCode}'");

        // 3. التحقق
        if (string.IsNullOrEmpty(cachedOtp) || cachedOtp.Trim() != request.OtpCode.Trim())
        {
            return new VerifyOtpResultDto { IsSuccess = false, Message = "Invalid or expired OTP" };
        }

        // 4. مسح الكاش وتوليد الـ Reset Token
        await _cache.RemoveAsync(cacheKey);
        var resetToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{cleanPhone}:{Guid.NewGuid()}:reset"));
        await _cache.SetAsync($"reset:{resetToken}", cleanPhone, TimeSpan.FromMinutes(15));

        // 5. جلب بيانات اليوزر
        var spec = new UserForLoginSpec(cleanPhone);
        var user = await _uow.Users.GetBySpecAsync(spec);

        if (user == null)
        {
            return new VerifyOtpResultDto
            {
                IsSuccess = true,
                ResetToken = resetToken,
                Message = "OTP verified. Profile not found."
            };
        }

        // 6. تحديد الـ Role والاسم
        var role = user.Driver != null ? "Driver" : user.Manager != null ? "Manager" : "User";
        var fullName = $"{user.FirstName} {user.LastName}";
        var token = _jwt.GenerateToken(user.PhoneNumber, role, fullName);

        // 7. ✅ الحل النهائي: إنشاء الـ DTO يدوياً (بدل الماپر اللي بيضرب)
        return new VerifyOtpResultDto
        {
            IsSuccess = true,
            Message = "Success",
            Token = token,
            ResetToken = resetToken, // كدة مش هيطلع null أبداً
            Role = role,
            FullName = fullName
        };
    }
}