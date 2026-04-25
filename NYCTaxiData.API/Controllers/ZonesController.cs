//using Microsoft.AspNetCore.Mvc;
//using NYCTaxiData.API.Contracts;

//namespace NYCTaxiData.API.Controllers
//{
//    [ApiController]
//    [Route("api/v1/[controller]")]
//    public class ZonesController : ControllerBase
//    {
//        // --------------------------------------------------------
//        // 1. Get All Zones (المضلعات الجغرافية - GeoJSON)
//        // --------------------------------------------------------
//        [HttpGet]
//        public IActionResult GetAllZones()
//        {
//            // في المستقبل، هذه البيانات يفضل أن تقرأ من (Redis Cache) وليس من الداتا بيز مباشرة لأنها لا تتغير.

//            // Mock Data Response (GeoJSON Format for Deck.gl)
//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "Zones geometry retrieved successfully",
//                Data = new
//                {
//                    type = "FeatureCollection",
//                    features = new[]
//                    {
//                        new
//                        {
//                            type = "Feature",
//                            properties = new { zoneId = "104-A", zoneName = "Lower Manhattan", borough = "Manhattan", h3Index = "892a10089ebffff", resolution = 9 },
//                            geometry = new
//                            {
//                                type = "Polygon",
//                                coordinates = new[]
//                                {
//                                    new[]
//                                    {
//                                        new[] { -74.016, 40.715 },
//                                        new[] { -74.006, 40.718 },
//                                        new[] { -73.996, 40.715 },
//                                        new[] { -73.996, 40.705 },
//                                        new[] { -74.006, 40.702 },
//                                        new[] { -74.016, 40.705 },
//                                        new[] { -74.016, 40.715 } // نقطة الإغلاق
//                                    }
//                                }
//                            }
//                        },
//                        new
//                        {
//                            type = "Feature",
//                            properties = new { zoneId = "105-B", zoneName = "Midtown South", borough = "Manhattan", h3Index = "892a100cb3bffff", resolution = 9 },
//                            geometry = new
//                            {
//                                type = "Polygon",
//                                coordinates = new[]
//                                {
//                                    new[]
//                                    {
//                                        new[] { -73.985, 40.748 },
//                                        new[] { -73.975, 40.751 },
//                                        new[] { -73.965, 40.748 },
//                                        new[] { -73.965, 40.738 },
//                                        new[] { -73.975, 40.735 },
//                                        new[] { -73.985, 40.738 },
//                                        new[] { -73.985, 40.748 }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            });
//        }

//        // --------------------------------------------------------
//        // 2. Get Live Demand Heatmap (خريطة الحرارة اللحظية)
//        // --------------------------------------------------------
//        [HttpGet("live-demand")]
//        public IActionResult GetLiveDemand()
//        {
//            // هذه الـ Endpoint سيتم استدعاؤها مرة واحدة عند فتح الشاشة، وبعدها يتولى SignalR تحديث البيانات.

//            // Mock Data Response
//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "Live demand metrics retrieved",
//                Data = new[]
//                {
//                    new { zoneId = "104-A", h3Index = "892a10089ebffff", demandLevel = "CRITICAL", surgeMultiplier = 2.5m, availableFleet = 24, estimatedWaitTime = "12 mins", trend = "Rising Fast" },
//                    new { zoneId = "105-B", h3Index = "892a100cb3bffff", demandLevel = "NORMAL", surgeMultiplier = 1.0m, availableFleet = 145, estimatedWaitTime = "2 mins", trend = "Stable" },
//                    new { zoneId = "106-C", h3Index = "892a100d2abffff", demandLevel = "ELEVATED", surgeMultiplier = 1.4m, availableFleet = 65, estimatedWaitTime = "6 mins", trend = "Rising" }
//                }
//            });
//        }

//        // --------------------------------------------------------
//        // 3. Get Specific Zone Insights (تحليل منطقة معينة عند الضغط عليها)
//        // --------------------------------------------------------
//        [HttpGet("{zoneId}/insights")]
//        public IActionResult GetZoneInsights([FromRoute] string zoneId)
//        {
//            // Mock Data Response
//            return Ok(new APIResponse<object>
//            {
//                IsSuccess = true,
//                Message = "Zone insights retrieved successfully",
//                Data = new
//                {
//                    zoneId = zoneId,
//                    zoneName = zoneId == "104-A" ? "Lower Manhattan" : "Selected Zone",
//                    h3Index = "892a10089ebffff",
//                    currentMetrics = new
//                    {
//                        activeTaxis = 24,
//                        waitingPassengers = 150,
//                        averageWaitTime = "12 mins",
//                        liveRevenue = 12450.00m,
//                        driverEfficiency = "88%"
//                    },
//                    demandVelocity = new
//                    {
//                        timeLabels = new[] { "14:00", "14:15", "14:30", "14:45", "15:00" },
//                        actualRequests = new[] { 200, 250, 310, 400, 480 },
//                        aiForecast = new[] { 210, 240, 300, 410, 500 }
//                    },
//                    topIntersections = new[]
//                    {
//                        new { name = "Wall St & Broadway", pickupCount = 45 },
//                        new { name = "Fulton St & William St", pickupCount = 32 }
//                    }
//                }
//            });
//        }
//    }
//}