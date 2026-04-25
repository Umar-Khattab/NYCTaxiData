// NYCTaxiData.Application/Common/Interfaces/IDispatchNotificationService.cs
using NYCTaxiData.Application.DTOs.Tracking;
using NYCTaxiData.Application.DTOs.Tracking;

namespace NYCTaxiData.Application.Common.Interfaces;

public interface IDispatchNotificationService
{
    // ✅ بعت Dispatch لسائق معين
    Task SendDispatchToDriverAsync(string phone, object notification, CancellationToken ct = default);


    // ✅ بعت لكل السائقين المتاحين
    Task BroadcastDispatchAsync(DispatchNotificationDto notification);

    // ✅ بعت تحديث حالة الرحلة
    Task NotifyTripStatusAsync(int tripId, string status);

    // ✅ بعت AI Order للسائق
    Task SendAiDispatchOrderAsync(string driverPhone, AiDispatchOrderDto order);
}