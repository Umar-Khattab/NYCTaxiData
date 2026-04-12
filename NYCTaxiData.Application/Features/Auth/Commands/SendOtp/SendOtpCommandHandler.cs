using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;

public class SendOtpCommandHandler(
	IUnitOfWork _uow,
	ICacheService _cache,
	ISmsService _sms)
	: IRequestHandler<SendOtpCommand, ResultDto>
{
	public async Task<ResultDto> Handle(SendOtpCommand request, CancellationToken cancellationToken)
	{
		// ✅ التأكد من وجود المستخدم بالـ Specification
		var spec = new UserByPhoneSpec(request.PhoneNumber);
		var userExists = await _uow.Users.AnyWithSpecAsync(spec, cancellationToken); // استخدم AnyWithSpecAsync لضمان التوافق

		if (!userExists)
			return new ResultDto { IsSuccess = false, Message = "Phone number not registered" };

		// توليد OTP مكون من 6 أرقام
		var otp = new Random().Next(100000, 999999).ToString();
		var cacheKey = $"otp:{request.PhoneNumber}";

		// حفظ في الكاش لمدة 5 دقائق
		await _cache.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(5));

		// إرسال الرسالة
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