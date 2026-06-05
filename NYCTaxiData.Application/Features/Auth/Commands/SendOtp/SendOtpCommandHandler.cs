using MediatR;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Interfaces.Services; 
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Domain.Specifications.Users; 
using System.Security.Cryptography;   
namespace NYCTaxiData.Application.Features.Auth.Commands.SendOtp
{
    public class SendOtpCommandHandler(
        IUnitOfWork _uow,
        ICacheService _cache,
        ICurrentUserService currentUser,
        ISmsService _sms)
        : IRequestHandler<SendOtpCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            // 1. لا تأخذ الرقم من الـ request. قم بإحضاره من الـ Current User
            // (بافتراض أنك تستخدم _userContext لتعريف السائق الحالي)
            var userId = currentUser.UserId;
            var user = await _uow.Users.GetByIdAsync(userId);

            if (string.IsNullOrEmpty(userId.ToString()))
                return new ResultDto { IsSuccess = false, Message = "Unauthorized" };

            // 2. استخدم الرقم المخزن في الداتابيز (المصدر الموثوق)
            var phoneToSend = user.PhoneNumber;

            // 3. إنشاء الـ OTP وتخزينه في الكاش
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var cacheKey = $"otp:{user.Id}"; // استخدم الـ ID كـ Key أفضل من الرقم

            await _cache.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(5));

            // 4. الإرسال للرقم الموجود في الداتابيز
            var smsSent = await _sms.SendSmsAsync(phoneToSend, $"Your code: {otp}");

            return new ResultDto { IsSuccess = true, Message = "OTP sent" };
        }
    }
}