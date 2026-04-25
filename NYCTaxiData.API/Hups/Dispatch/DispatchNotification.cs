// NYCTaxiData.Infrastructure/Services/DispatchNotificationService.cs
using Microsoft.AspNetCore.SignalR;
using NYCTaxiData.API.Hups.Dispatch;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.Tracking;

namespace NYCTaxiData.API.Hups.Dispatch;
public class DispatchNotification : IDispatchNotificationService
{
    private readonly IHubContext<DispatchHub> _hubContext;

    public DispatchNotification(IHubContext<DispatchHub> hubContext)
    {
        _hubContext = hubContext;
    }

    // ✅ بعت لسائق معين بالـ Phone
    public async Task SendDispatchToDriverAsync(string driverPhone, object notification, CancellationToken ct)
    {
        await _hubContext.Clients.All.SendAsync("NewDispatchOrder", notification, ct);

        // اطبع في الـ Debug console عشان تشوفها وأنت بتعمل Run
        System.Diagnostics.Debug.WriteLine($"[SignalR] Sent notification to all for driver: {driverPhone}");
    }

    // ✅ بعت لكل السائقين
    public async Task BroadcastDispatchAsync(DispatchNotificationDto notification)
    {
        await _hubContext.Clients
            .Group("Drivers")
            .SendAsync("NewDispatchOrder", notification);
    }

    // ✅ تحديث حالة الرحلة
    public async Task NotifyTripStatusAsync(int tripId, string status)
    {
        await _hubContext.Clients
            .All
            .SendAsync("TripStatusUpdated", new
            {
                TripId = tripId,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            });
    }

    // ✅ AI Dispatch Order
    public async Task SendAiDispatchOrderAsync(string driverPhone, AiDispatchOrderDto order)
    {
        Console.WriteLine($"[SignalR] Sending Notification to Phone: {driverPhone}"); // 👈 ضيف ده
        await _hubContext.Clients
            .User(driverPhone)
            .SendAsync("AiDispatchOrder", order);
    }
}