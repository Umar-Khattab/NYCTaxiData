using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.Common.Specifications.Auth;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;
using System.Security.Cryptography;

namespace NYCTaxiData.Application.Features.Auth.Commands.SendOtp
{
    public class SendOtpCommandHandler(
        IUnitOfWork _uow,
        ICacheService _cache,
        ISmsService _sms)
        : IRequestHandler<SendOtpCommand, ResultDto>
    {
        public async Task<ResultDto> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            var cleanPhone = request.PhoneNumber.Trim().Replace(" ", "");
             
            if (cleanPhone.StartsWith("20")) cleanPhone = "0" + cleanPhone.Substring(2);

            var spec = new UserByPhoneSpec(cleanPhone);
            var userExists = await _uow.Users.AnyWithSpecAsync(spec, cancellationToken);
            if (!userExists)
            {
                return new ResultDto { IsSuccess = false, Message = "Phone number not registered" };
            }

            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var cacheKey = $"otp:{request.PhoneNumber}";
             
            await _cache.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(5));
             
            var smsSent = await _sms.SendSmsAsync(
                request.PhoneNumber,
                $"Your NYCTaxi OTP code is: {otp}. Valid for 5 minutes.");
             
            if (!smsSent)
            {
                await _cache.RemoveAsync(cacheKey);
                return new ResultDto { IsSuccess = false, Message = "Failed to send OTP" };
            }

            return new ResultDto { IsSuccess = true, Message = "OTP sent successfully" };
        }
    }
}