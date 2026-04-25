//using Microsoft.AspNetCore.Mvc;
//using NYCTaxiData.API.Contracts;

//namespace NYCTaxiData.API.Controllers
//{
//    [ApiController]
//    [Route("api/v1/[controller]")]
//    public class AnalyticsController : ControllerBase
//    {
//        // --------------------------------------------------------
//        // 1. Get Top-Level Operational KPIs (الإحصائيات العلوية للداشبورد)
//        // --------------------------------------------------------
//        [HttpGet("kpis")]
//        public IActionResult GetKpis()
//        {
//            // هذه الأرقام عادة يتم تجميعها في الخلفية (Background Worker) وتُخزن في Redis

//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "Operational KPIs retrieved",
//                Data = new
//                {
//                    totalActiveFleet = 4521,
//                    availableUnits = 432,
//                    onTripUnits = 4089,
//                    pendingDispatches = 15,
//                    criticalAlertsCount = 3,
//                    todayTotalRevenue = 154200.50m, // حرف m للـ decimal
//                    averageWaitTimeCitywide = "5m 30s"
//                }
//            });
//        }

//        // --------------------------------------------------------
//        // 2. Get Demand Velocity Chart Data (بيانات الرسم البياني D3.js)
//        // --------------------------------------------------------
//        [HttpGet("demand-velocity")]
//        public IActionResult GetDemandVelocity([FromQuery] string zoneId = "104-A", [FromQuery] int hours = 4)
//        {
//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "Demand velocity data retrieved",
//                Data = new
//                {
//                    zoneId = zoneId,
//                    timeLabels = new[] { "10:00", "11:00", "12:00", "13:00", "14:00" },
//                    actualDemandRpm = new[] { 120, 180, 250, 400, 410 },
//                    aiForecastRpm = new[] { 115, 175, 260, 390, 420 },
//                    modelAccuracyPercentage = 96.5m
//                }
//            });
//        }

//        // --------------------------------------------------------
//        // 3. Get System Thresholds (جلب إعدادات النظام الحالية)
//        // --------------------------------------------------------
//        [HttpGet("thresholds")]
//        public IActionResult GetThresholds()
//        {
//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "System thresholds retrieved",
//                Data = new
//                {
//                    rpmThresholds = new
//                    {
//                        normalMax = 300,
//                        elevatedMax = 700,
//                        criticalMin = 701
//                    },
//                    surgeMultipliers = new
//                    {
//                        elevated = 1.5m,
//                        critical = 2.4m
//                    },
//                    mapPreferences = new
//                    {
//                        defaultStyle = "Dark Matter",
//                        show3DBuildings = true,
//                        showTrafficLayer = true
//                    }
//                }
//            });
//        }

//        // --------------------------------------------------------
//        // 4. Update System Thresholds (تحديث قواعد النظام ديناميكياً)
//        // --------------------------------------------------------
//        [HttpPut("thresholds")]
//        public IActionResult UpdateThresholds([FromBody] UpdateThresholdsRequest request)
//        {
//            // في المستقبل، تحديث هذه القيم سيقوم بعمل Broadcast عبر الـ SignalR
//            // لتحديث ألوان الخريطة عند جميع المستخدمين في نفس اللحظة!

//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "System thresholds updated successfully and applied to live environment",
//                Data = null // لا نحتاج لإرجاع بيانات، رسالة النجاح تكفي
//            });
//        }
//    }

//    // ========================================================================
//    // 📦 Request DTOs (Records)
//    // ========================================================================

//    // استخدمنا الـ Records المتداخلة (Nested) لتطابق شكل الـ JSON القادم من الـ Frontend
//    public record UpdateThresholdsRequest(
//        RpmThresholdsDto RpmThresholds,
//        SurgeMultipliersDto SurgeMultipliers,
//        MapPreferencesDto MapPreferences
//    );

//    public record RpmThresholdsDto(
//        int NormalMax,
//        int ElevatedMax,
//        int CriticalMin
//    );

//    public record SurgeMultipliersDto(
//        decimal Elevated,
//        decimal Critical
//    );

//    public record MapPreferencesDto(
//        string DefaultStyle,
//        bool Show3DBuildings,
//        bool ShowTrafficLayer
//    );
//}