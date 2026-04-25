 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NYCTaxiData.API.Hubs;

[Authorize]
public class TaxiHub : Hub
{
    // لما السائق يتوصل
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var fullName = Context.User?.FindFirst("FullName")?.Value;

        // أضف المستخدم لـ Group بناءً على الـ Role
        if (role == "Driver")
            await Groups.AddToGroupAsync(Context.ConnectionId, "Drivers");
        else if (role == "Manager")
            await Groups.AddToGroupAsync(Context.ConnectionId, "Managers");

        await base.OnConnectedAsync();
    }

    // لما المستخدم يقطع الاتصال
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (role == "Driver")
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Drivers");
        else if (role == "Manager")
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Managers");

        await base.OnDisconnectedAsync(exception);
    }

    // السائق يبعت location update
    public async Task SendDriverLocation(double lat, double lng)
    {
        var driverId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Clients.Group("Managers").SendAsync("DriverLocationUpdated", new
        {
            DriverId = driverId,
            Latitude = lat,
            Longitude = lng,
            Timestamp = DateTime.UtcNow
        });
    }

    // المدير يبعت dispatch command للسائق
    public async Task SendDispatchCommand(string driverPhone, string zoneId, string zoneName)
    {
        await Clients.User(driverPhone).SendAsync("DispatchCommandReceived", new
        {
            ZoneId = zoneId,
            ZoneName = zoneName,
            Timestamp = DateTime.UtcNow
        });
    }

    // تحديث حالة الرحلة لكل المتصلين
    public async Task UpdateTripStatus(int tripId, string status)
    {
        await Clients.All.SendAsync("TripStatusUpdated", new
        {
            TripId = tripId,
            Status = status,
            Timestamp = DateTime.UtcNow
        });
    }
}