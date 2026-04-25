// NYCTaxiData.API/Hubs/LiveTrackingHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.DTOs.Tracking;
using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.API.Hubs;
 [Authorize]
public class LiveTrackingHub : Hub
{
    private readonly TaxiDbContext _context;
    private static readonly Dictionary<string, DriverLocationInfo> _activeDrivers = new();

    public LiveTrackingHub(TaxiDbContext context)
    {
        _context = context;
    }

    // ===== Connection =====
    public override async Task OnConnectedAsync()
    { 
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value
                   ?? Context.User?.FindFirst("role")?.Value
                   ?? Context.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst(ClaimTypes.Name)?.Value;
         

        if (role == "Driver")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Drivers"); 
        }
        else if (role == "Manager")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Managers"); 

            if (_activeDrivers.Any())
            {
                await Clients.Caller.SendAsync("ActiveDrivers", _activeDrivers.Values);
            }
        }

        await base.OnConnectedAsync();
    }

    // ===== Disconnection =====
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role == "Driver" && phone != null)
        {
            // شيل السائق من الـ Active List
            _activeDrivers.Remove(phone);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Drivers");

            // ابعت للمدير إن السائق قطع
            await Clients.Group("Managers").SendAsync("DriverDisconnected", new
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

    // ===== Driver يبعت GPS =====
    public async Task UpdateLocation(double latitude, double longitude, double? speed = null, double? heading = null)
    {
        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(phone)) return;
         
        if (_activeDrivers.TryGetValue(phone, out var driverInfo))
        {
            driverInfo.Latitude = latitude;
            driverInfo.Longitude = longitude;
            driverInfo.Speed = speed;
            driverInfo.Heading = heading;
            driverInfo.UpdatedAt = DateTime.UtcNow;

            await Clients.Group("Managers").SendAsync("DriverLocationUpdated", driverInfo);
            await Clients.Caller.SendAsync("UpdateReceived", new { status = "Success (Cache)" });
            return;
        }

        // 2️⃣ لو أول مرة يبعت، روح هاته من الـ DB (بإستخدام الـ Trim عشان دقة البحث)
        var driver = await _context.Drivers
            .Include(d => d.IdNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.IdNavigation.Phonenumber.Trim() == phone.Trim());

        if (driver != null)
        {
            driverInfo = new DriverLocationInfo
            {
                DriverId = driver.Id, // 👈 هنا الـ ID الحقيقي من الـ DB
                DriverName = driver.Fullname ?? "Unknown Driver",
                Phone = phone,
                Latitude = latitude,
                Longitude = longitude,
                Status = driver.Status.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            _activeDrivers[phone] = driverInfo;
            await Clients.Group("Managers").SendAsync("DriverLocationUpdated", driverInfo);
            await Clients.Caller.SendAsync("UpdateReceived", new { status = "Success (DB)" });
        }
        else
        {
            // 💡 حالة الطوارئ: لو السواق مش في الـ DB، اديله ID وهمي عشان ميظهرش أصفار 
            var tempInfo = new DriverLocationInfo
            {
                DriverId = Guid.NewGuid(),
                DriverName = "New Driver (" + phone + ")",
                Phone = phone,
                Latitude = latitude,
                Longitude = longitude,
                Status = "Available",
                UpdatedAt = DateTime.UtcNow
            };
            _activeDrivers[phone] = tempInfo;
            await Clients.Group("Managers").SendAsync("DriverLocationUpdated", tempInfo);
        }
    }

    // ===== Manager يطلب كل السائقين =====
    public async Task GetAllDriverLocations()
    { 
         
        if (!_activeDrivers.Any())
        {
            var activeDriversFromDb = await _context.Drivers
                .Include(d => d.IdNavigation)
                .AsNoTracking()
                .Where(d => d.Status != CurrentStatus.Offline)
                .ToListAsync();

            foreach (var d in activeDriversFromDb)
            {
                _activeDrivers[d.IdNavigation.Phonenumber] = new DriverLocationInfo
                {
                    Phone = d.IdNavigation.Phonenumber,
                    DriverName = d.Fullname,
                    Status = d.Status.ToString()
                };
            }
        }
         
        var result = _activeDrivers.Values.ToList();

        await Clients.Caller.SendAsync("ActiveDrivers", result); 
    }

    // ===== السائق يغير Status =====
    public async Task UpdateDriverStatus(string status)
    {
        var phone = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

        if (role != "Driver" || string.IsNullOrEmpty(phone)) return;
         
        if (!_activeDrivers.ContainsKey(phone))
        {
            _activeDrivers[phone] = new DriverLocationInfo
            {
                Phone = phone,
                DriverName = Context.User?.FindFirst("FullName")?.Value ?? "Unknown"
            };
        }

        _activeDrivers[phone].Status = status;
        _activeDrivers[phone].UpdatedAt = DateTime.UtcNow;
         
        await Clients.Group("Managers").SendAsync("DriverStatusUpdated", new
        {
            Phone = phone,
            Status = status,
            UpdatedAt = DateTime.UtcNow
        });
    }

    public async Task SendMessageToDriver(string driverPhone, string message)
    { 
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            From = "Manager: " + Context.User.Identity.Name,
            Text = message
        });
    }
}