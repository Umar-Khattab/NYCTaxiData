using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Controllers.Base;
using NYCTaxiData.Application.Features.Zones.Queries.CompareZones;
using NYCTaxiData.Application.Features.Zones.Queries.GetAllZones;
using NYCTaxiData.Application.Features.Zones.Queries.GetDriverDistribution;
using NYCTaxiData.Application.Features.Zones.Queries.GetHeatmapData;
using NYCTaxiData.Application.Features.Zones.Queries.GetHighStockoutZones;
using NYCTaxiData.Application.Features.Zones.Queries.GetPeakHours;
using NYCTaxiData.Application.Features.Zones.Queries.GetRecommendedZones;
using NYCTaxiData.Application.Features.Zones.Queries.GetZoneById;
using NYCTaxiData.Application.Features.Zones.Queries.GetZoneHistory;
using NYCTaxiData.Application.Features.Zones.Queries.GetZoneInsights;
using NYCTaxiData.Application.Features.Zones.Queries.GetZoneMetadata;
using NYCTaxiData.Application.Features.Zones.Queries.GetZoneStatistics;
using NYCTaxiData.Application.Features.Zones.Queries.GetZoneTrends;
using NYCTaxiData.Application.Features.Zones.Queries.GetTopDemandZones;
using NYCTaxiData.Application.Features.Zones.Queries.GetTopRevenueZones;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NYCTaxiData.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ZonesController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllZones()
        {
            return HandleResult(await Mediator.Send(new GetAllZonesQuery()));
        }

        [HttpGet("metadata")]
        public async Task<IActionResult> GetZoneMetadata()
        {
            return HandleResult(await Mediator.Send(new GetZoneMetadataQuery()));
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetOverallStatistics()
        {
            return HandleResult(await Mediator.Send(new GetZoneStatisticsQuery(null)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetZoneById(int id)
        {
            return HandleResult(await Mediator.Send(new GetZoneByIdQuery(id)));
        }

        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetZoneStatistics(int id)
        {
            return HandleResult(await Mediator.Send(new GetZoneStatisticsQuery(id)));
        }

        [HttpGet("heatmap")]
        public async Task<IActionResult> GetHeatmapData()
        {
            return HandleResult(await Mediator.Send(new GetHeatmapDataQuery()));
        }

        [HttpGet("compare")]
        public async Task<IActionResult> CompareZones([FromQuery] List<int> zoneIds)
        {
            return HandleResult(await Mediator.Send(new CompareZonesQuery(zoneIds)));
        }

        [HttpGet("recommended")]
        public async Task<IActionResult> GetRecommendedZones([FromQuery] int limit = 10)
        {
            return HandleResult(await Mediator.Send(new GetRecommendedZonesQuery(limit)));
        }

        [HttpGet("trends")]
        public async Task<IActionResult> GetZoneTrends([FromQuery] int? zoneId, [FromQuery] string trendType = "hourly")
        {
            return HandleResult(await Mediator.Send(new GetZoneTrendsQuery(zoneId, trendType)));
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetZoneHistory([FromQuery] int? zoneId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return HandleResult(await Mediator.Send(new GetZoneHistoryQuery(zoneId, startDate, endDate)));
        }

        [HttpGet("peak-hours")]
        public async Task<IActionResult> GetPeakHours([FromQuery] int? zoneId)
        {
            return HandleResult(await Mediator.Send(new GetPeakHoursQuery(zoneId)));
        }

        [HttpGet("{id}/insights")]
        public async Task<IActionResult> GetZoneInsights(int id)
        {
            return HandleResult(await Mediator.Send(new GetZoneInsightsQuery(id)));
        }

        [HttpGet("driver-distribution")]
        public async Task<IActionResult> GetDriverDistribution()
        {
            return HandleResult(await Mediator.Send(new GetDriverDistributionQuery()));
        }

        [HttpGet("top-demand")]
        public async Task<IActionResult> GetTopDemandZones([FromQuery] int limit = 10)
        {
            return HandleResult(await Mediator.Send(new GetTopDemandZonesQuery(limit)));
        }

        [HttpGet("top-revenue")]
        public async Task<IActionResult> GetTopRevenueZones([FromQuery] int limit = 10)
        {
            return HandleResult(await Mediator.Send(new GetTopRevenueZonesQuery(limit)));
        }

        [HttpGet("high-stockout")]
        public async Task<IActionResult> GetHighStockoutZones([FromQuery] int limit = 10)
        {
            return HandleResult(await Mediator.Send(new GetHighStockoutZonesQuery(limit)));
        }
    }
}