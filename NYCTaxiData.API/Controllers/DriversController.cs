//using Microsoft.AspNetCore.Mvc;
//using NYCTaxiData.API.Contracts;

//namespace NYCTaxiData.API.Controllers
//{
//    [ApiController]
//    [Route("api/v1/[controller]")]
//    public class DriversController : ControllerBase
//    {
//        // --------------------------------------------------------
//        // 1. Update Driver Status
//        // --------------------------------------------------------
//        [HttpPut("{id}/status")]
//        public IActionResult UpdateStatus(string id, [FromBody] UpdateDriverStatusRequest request)
//        {
//            // Mock Data Response
//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "Driver status updated successfully",
//                Data = new
//                {
//                    driverId = id,
//                    status = request.Status,
//                    updatedAt = DateTime.UtcNow
//                }
//            });
//        }

//        // --------------------------------------------------------
//        // 2. Get Active Fleet
//        // --------------------------------------------------------
//        [HttpGet("active")]
//        public IActionResult GetActiveFleet()
//        {
//            // Mock Data Response
//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "Active fleet retrieved",
//                Data = new[]
//                {
//                    new { driverId = "DRV-1024", name = "Mostafa Ibrahim", status = "Available", vehicle = "NY-7721", lat = 40.7580, lng = -73.9855 },
//                    new { driverId = "DRV-2055", name = "Mohammed Ahmed", status = "On-Trip", vehicle = "NY-E442", lat = 40.6928, lng = -73.9903 },
//                    new { driverId = "DRV-3088", name = "Omar Khattab", status = "Available", vehicle = "NY-X901", lat = 40.7128, lng = -74.0060 }
//                }
//            });
//        }

//        // --------------------------------------------------------
//        // 3. Get Shift Statistics
//        // --------------------------------------------------------
//        [HttpGet("{id}/shift-stats")]
//        public IActionResult GetShiftStatistics(string id)
//        {
//            // Mock Data Response
//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "Shift statistics retrieved",
//                Data = new
//                {
//                    shiftId = "SHF-88992",
//                    driverId = id,
//                    startTime = DateTime.UtcNow.AddHours(-6.5), // بدأ من 6 ساعات ونص
//                    hoursActive = 6.5,
//                    totalEarnings = 185.50,
//                    tripsCompleted = 12,
//                    idleTimeMinutes = 45
//                }
//            });
//        }

//        // --------------------------------------------------------
//        // 4. Offline Data Batch Sync
//        // --------------------------------------------------------
//        [HttpPost("sync-offline")]
//        public IActionResult SyncOfflineTrips([FromBody] SyncOfflineRequest request)
//        {
//            // هنا في المستقبل سنقوم بفتح Transaction في قاعدة البيانات لحفظ كل الرحلات

//            // Mock Data Response
//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "Offline data synchronized successfully",
//                Data = new
//                {
//                    syncedTripsCount = request.OfflineTrips.Count,
//                    failedTrips = Array.Empty<string>() // لا يوجد رحلات فشلت في هذا السيناريو
//                }
//            });
//        }
//    }

//    // ========================================================================
//    // 📦 Request DTOs (Data Transfer Objects)
//    // نضعها هنا مؤقتاً في Sprint 1 لكي يقرأها Swagger.
//    // في Sprint 2، يجب نقل هذه الـ Records إلى طبقة الـ Application (مثلاً مجلد Features/Drivers/Commands)
//    // ========================================================================

//    public record UpdateDriverStatusRequest(
//        string Status,
//        double CurrentLat,
//        double CurrentLng
//    );

//    public record SyncOfflineRequest(
//        string DriverId,
//        List<OfflineTripDto> OfflineTrips
//    );

//    public record OfflineTripDto(
//        string LocalTripId,
//        double PickupLat,
//        double PickupLng,
//        double DropoffLat,
//        double DropoffLng,
//        DateTime StartTime,
//        DateTime EndTime,
//        decimal CalculatedFare,
//        double DistanceMiles
//    );
//}