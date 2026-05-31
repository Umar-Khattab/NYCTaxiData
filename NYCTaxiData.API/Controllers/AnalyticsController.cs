using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Controllers.Base;
using NYCTaxiData.Application.Features.Analytics.Commands.UpdateSystemThresholds;
using NYCTaxiData.Application.Features.Analytics.Queries.GetDemandVelocityChart;
using NYCTaxiData.Application.Features.Analytics.Queries.GetSystemThresholds;
using NYCTaxiData.Application.Features.Analytics.Queries.GetTopLevelKpis;

namespace NYCTaxiData.API.Controllers;

/// <summary>
/// Provides operational analytics endpoints: KPIs, demand velocity, and system threshold configuration.
/// </summary>
[Authorize(Roles = "Admin,Dispatcher")]
[ApiController]
[Route("api/v1/[controller]")]
public class AnalyticsController : BaseController
{
    /// <summary>
    /// Returns top-level KPI metrics for the operations dashboard (drivers, revenue, wait times).
    /// </summary>
    [HttpGet("kpis")]
    [Authorize(Roles = "Admin,Dispatcher,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopLevelKpis()
        => HandleResult(await Mediator.Send(new GetTopLevelKpisQuery()));

    /// <summary>
    /// Returns demand velocity chart data for a zone and date range.
    /// </summary>
    [HttpGet("demand-velocity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDemandVelocity([FromQuery] GetDemandVelocityChartQuery query)
        => HandleResult(await Mediator.Send(query));

    /// <summary>
    /// Returns current system configuration thresholds (surge multipliers, dispatch radii).
    /// </summary>
    [HttpGet("thresholds")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSystemThresholds()
        => HandleResult(await Mediator.Send(new GetSystemThresholdsQuery()));

    /// <summary>
    /// Updates system configuration thresholds. Restricted to Admin role.
    /// </summary>
    [HttpPut("thresholds")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSystemThresholds([FromBody] UpdateSystemThresholdsCommand command)
        => HandleResult(await Mediator.Send(command));
}