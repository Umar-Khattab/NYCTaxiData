// NYCTaxiData.API/Hubs/DispatchHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NYCTaxiData.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using NYCTaxiData.Application.DTOs.Tracking;

namespace NYCTaxiData.API.Hups.Dispatch;

[Authorize]
public class DispatchHub : Hub
{
    private readonly TaxiDbContext _context;

    // ✅ تخزين ConnectionId لكل Driver
    private static readonly Dictionary<string, string> _driverConnections = new();

    public DispatchHub(TaxiDbContext context)
    {
        _context = context;
    }

    // ===== Connection =====
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role == "Driver" && !string.IsNullOrEmpty(phone))
        {
            // ✅ حفظ الـ ConnectionId للسائق
            _driverConnections[phone] = Context.ConnectionId;

            await Groups.AddToGroupAsync(Context.ConnectionId, "Drivers");

            // أخبر المدير إن سائق متصل
            await Clients.Group("Managers").SendAsync("DriverOnline", new
            {
                Phone = phone,
                ConnectedAt = DateTime.UtcNow
            });
        }
        else if (role == "Manager")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Managers");

            // ابعتله السائقين المتصلين حالياً
            await Clients.Caller.SendAsync("OnlineDrivers", _driverConnections.Keys);
        }

        await base.OnConnectedAsync();
    }

    // ===== Disconnection =====
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role == "Driver" && !string.IsNullOrEmpty(phone))
        {
            _driverConnections.Remove(phone);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Drivers");

            await Clients.Group("Managers").SendAsync("DriverOffline", new
            {
                Phone = phone,
                DisconnectedAt = DateTime.UtcNow
            });
        }
        else if (role == "Manager")
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Managers");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ===== السائق يقبل الـ Dispatch =====
    public async Task AcceptDispatch(string dispatchId)
    {
        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (role != "Driver" || string.IsNullOrEmpty(phone)) return;

        // ابعت للمدير إن السائق قبل
        await Clients.Group("Managers").SendAsync("DispatchAccepted", new
        {
            DispatchId = dispatchId,
            DriverPhone = phone,
            AcceptedAt = DateTime.UtcNow
        });

        // confirm للسائق نفسه
        await Clients.Caller.SendAsync("DispatchConfirmed", new
        {
            DispatchId = dispatchId,
            Status = "Accepted",
            Message = "Dispatch order confirmed successfully"
        });
    }

    // ===== السائق يرفض الـ Dispatch =====
    public async Task RejectDispatch(string dispatchId, string reason)
    {
        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (role != "Driver" || string.IsNullOrEmpty(phone)) return;

        await Clients.Group("Managers").SendAsync("DispatchRejected", new
        {
            DispatchId = dispatchId,
            DriverPhone = phone,
            Reason = reason,
            RejectedAt = DateTime.UtcNow
        });

        await Clients.Caller.SendAsync("DispatchRejectionConfirmed", new
        {
            DispatchId = dispatchId,
            Status = "Rejected"
        });
    }

    // ===== السائق يوصل للـ Pickup =====
    public async Task ArrivedAtPickup(int tripId)
    {
        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Clients.Group("Managers").SendAsync("DriverArrivedAtPickup", new
        {
            TripId = tripId,
            Phone = phone,
            ArrivedAt = DateTime.UtcNow
        });
    }

    // ===== السائق يكمّل الرحلة =====
    public async Task TripCompleted(int tripId)
    {
        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Clients.Group("Managers").SendAsync("TripCompleted", new
        {
            TripId = tripId,
            Phone = phone,
            CompletedAt = DateTime.UtcNow
        });
    }
}