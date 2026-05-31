using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Controllers.Base;
using NYCTaxiData.Application.Features.Drivers.Commands.SyncOfflineData;
using NYCTaxiData.Application.Features.Drivers.Commands.UpdateDriverStatus;
using NYCTaxiData.Application.Features.Drivers.Queries.GetActiveFleet;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverAnalytics;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverEarnings;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;
using NYCTaxiData.Application.Features.Drivers.Queries.GetShiftStatistics;

namespace NYCTaxiData.API.Controllers;

/// <summary>
/// Provides endpoints for driver management, fleet tracking, and driver analytics.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DriversController : BaseController
{
    /// <summary>
    /// Returns a paginated list of drivers with optional status and zone filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDriverList([FromQuery] GetDriverListQuery query, CancellationToken ct)
        => HandleResult(await Mediator.Send(query, ct));

    /// <summary>
    /// Returns a paginated list of currently active (non-offline) drivers.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActiveFleet([FromQuery] GetActiveFleetQuery query, CancellationToken ct)
        => HandleResult(await Mediator.Send(query, ct));

    /// <summary>
    /// Returns detailed profile and current statistics for a single driver.
    /// </summary>
    [HttpGet("{driverId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDriverProfile(Guid driverId, CancellationToken ct)
        => HandleResult(await Mediator.Send(new GetDriverProfileQuery(driverId), ct));

    /// <summary>
    /// Returns shift statistics for a driver within a specific time window.
    /// </summary>
    [HttpGet("{driverId:guid}/shift-stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetShiftStatistics(
        Guid driverId,
        [FromQuery] DateTime? shiftStartUtc,
        [FromQuery] DateTime? shiftEndUtc,
        CancellationToken ct)
        => HandleResult(await Mediator.Send(new GetShiftStatisticsQuery(driverId, shiftStartUtc, shiftEndUtc), ct));

    /// <summary>
    /// Updates a driver's availability status and current GPS coordinates.
    /// </summary>
    [HttpPut("{driverId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid driverId, [FromBody] UpdateDriverStatusRequest request, CancellationToken ct)
    {
        var command = new UpdateDriverStatusCommand(driverId, request.Status, request.CurrentLat, request.CurrentLng);
        return HandleResult(await Mediator.Send(command, ct));
    }

    /// <summary>
    /// Synchronizes batched offline trip data collected while the driver had no connectivity.
    /// </summary>
    [HttpPost("sync-offline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SyncOffline([FromBody] SyncOfflineDataCommand command, CancellationToken ct)
        => HandleResult(await Mediator.Send(command, ct));

    /// <summary>
    /// Returns performance analytics (earnings, trips, routes) for a driver over a date range.
    /// </summary>
    [HttpGet("analytics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] Guid driverId,
        [FromQuery] DateTime startRange,
        [FromQuery] DateTime endRange,
        CancellationToken ct)
        => HandleResult(await Mediator.Send(new GetDriverAnalyticsQuery(driverId, startRange, endRange), ct));

    /// <summary>
    /// Returns earnings breakdown for a driver for the specified period (day/week/month).
    /// </summary>
    [HttpGet("earnings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEarnings(
        [FromQuery] Guid driverId,
        [FromQuery] string period = "week",
        CancellationToken cancellationToken = default)
        => HandleResult(await Mediator.Send(new GetDriverEarningsQuery(driverId, period), cancellationToken));

    /// <summary>Request body for the UpdateStatus endpoint.</summary>
    public sealed record UpdateDriverStatusRequest(string Status, double CurrentLat, double CurrentLng);
}
