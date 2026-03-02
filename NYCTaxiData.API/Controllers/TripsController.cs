using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Contracts;

namespace NYCTaxiData.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TripsController : ControllerBase
    {
        // --------------------------------------------------------
        // 1. Start Trip
        // --------------------------------------------------------
        [HttpPost("start")]
        public IActionResult StartTrip([FromBody] StartTripRequest request)
        {
            // هنا في المستقبل سيتم إنشاء سجل في جدول الرحلات (Trips Table) وتغيير حالة السائق إلى On-Trip

            // Mock Data Response
            return StatusCode(201, new APIResponse<object>
            {
                IsSuccess = true,
                Message = "Trip started successfully",
                Data = new
                {
                    tripId = $"TRP-{new Random().Next(100000, 999999)}", // توليد ID وهمي للرحلة
                    status = "In-Progress",
                    driverStatus = "On-Trip"
                }
            });
        }

        // --------------------------------------------------------
        // 2. End Trip & Calculate Fare
        // --------------------------------------------------------
        [HttpPost("end")]
        public IActionResult EndTrip([FromBody] EndTripRequest request)
        {
            // هنا في المستقبل سيتم حساب الأجرة بناءً على المسافة، الوقت، والـ Surge Multiplier من الذكاء الاصطناعي

            // Mock Data Response
            return Ok(new APIResponse<object>
            {
                IsSuccess = true,
                Message = "Trip ended and fare calculated",
                Data = new
                {
                    tripId = request.TripId,
                    durationMinutes = 25,
                    baseFare = 12.50m,
                    surgeApplied = 1.5m,
                    totalFare = 18.75m,
                    driverStatus = "Available"
                }
            });
        }

        // --------------------------------------------------------
        // 3. Get Trip History (للسائق أو المدير)
        // --------------------------------------------------------
        [HttpGet("history")]
        public IActionResult GetTripHistory([FromQuery] string driverId, [FromQuery] int limit = 10, [FromQuery] int page = 1)
        {
            // Mock Data Response
            return Ok(new APIResponse<object>
            {
                IsSuccess = true,
                Message = "Trip history retrieved",
                Data = new
                {
                    currentPage = page,
                    totalPages = 5,
                    totalCount = 45,
                    items = new[]
                    {
                        new { tripId = "TRP-998877", pickupZone = "Midtown West", dropoffZone = "Financial District", distanceMiles = 4.2, totalFare = 18.75m, date = DateTime.UtcNow.AddHours(-2) },
                        new { tripId = "TRP-998876", pickupZone = "JFK Airport", dropoffZone = "Midtown West", distanceMiles = 15.1, totalFare = 55.00m, date = DateTime.UtcNow.AddDays(-1) }
                    }
                }
            });
        }

        // --------------------------------------------------------
        // 4. Manual Dispatch Action (أمر التوجيه من المدير)
        // --------------------------------------------------------
        [HttpPost("dispatch/manual")]
        public IActionResult ManualDispatch([FromBody] ManualDispatchRequest request)
        {
            // هنا في المستقبل سيتم إرسال SignalR Event أو Push Notification لتطبيق السائق

            // Mock Data Response
            return Ok(new APIResponse<object>
            {
                IsSuccess = true,
                Message = "Dispatch command sent to driver successfully",
                Data = new
                {
                    dispatchId = $"DSP-{new Random().Next(1000, 9999)}",
                    status = "Sent",
                    timestamp = DateTime.UtcNow
                }
            });
        }

        // --------------------------------------------------------
        // 5. Get Live Dispatch Feed (للوحة التحكم)
        // --------------------------------------------------------
        [HttpGet("dispatch/feed")]
        public IActionResult GetLiveDispatchFeed([FromQuery] int limit = 5)
        {
            // Mock Data Response
            return Ok(new APIResponse<object>
            {
                IsSuccess = true,
                Message = "Dispatch feed retrieved",
                Data = new[]
                {
                    new { dispatchId = "DSP-5001", driverName = "Mostafa Ibrahim", targetZone = "Lower Manhattan", status = "Accepted", timeElapsed = "2 mins ago" },
                    new { dispatchId = "DSP-5000", driverName = "Ahmed Khaled", targetZone = "Times Square", status = "Failed", timeElapsed = "15 mins ago" }
                }
            });
        }
    }

    // ========================================================================
    // 📦 Request DTOs (Data Transfer Objects)
    // ========================================================================

    public record StartTripRequest(
        string DriverId,
        double PickupLat,
        double PickupLng,
        string PickupZoneId,
        DateTime StartTime
    );

    public record EndTripRequest(
        string TripId,
        double DropoffLat,
        double DropoffLng,
        string DropoffZoneId,
        double DistanceMiles,
        DateTime EndTime
    );

    public record ManualDispatchRequest(
        string TargetZoneId,
        string TargetZoneName,
        int NumberOfUnits,
        string AssignedDriverId,
        string Priority, // e.g., "CRITICAL", "NORMAL"
        bool SmartRoutingEnabled
    );
}