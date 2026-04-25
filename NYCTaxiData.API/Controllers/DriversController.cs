using MediatR;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.Application.Features.Drivers.Commands.SyncOfflineData;
using NYCTaxiData.Application.Features.Drivers.Commands.UpdateDriverStatus;
using NYCTaxiData.Application.Features.Drivers.Queries.GetActiveFleet;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;
using NYCTaxiData.Application.Features.Drivers.Queries.GetShiftStatistics;

namespace NYCTaxiData.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DriversController : ControllerBase
{
    private readonly ISender _sender;

    public DriversController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves a paginated list of drivers with optional status and zone filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDriverList([FromQuery] GetDriverListQuery query)
    {
        var result = await _sender.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves a paginated list of active fleet drivers (non-offline).
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetActiveFleet([FromQuery] GetActiveFleetQuery query)
    {
        var result = await _sender.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves detailed profile information and current statistics for a single driver.
    /// </summary>
    [HttpGet("{driverId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDriverProfile(Guid driverId)
    {
        var result = await _sender.Send(new GetDriverProfileQuery(driverId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves shift statistics for a driver within a time window.
    /// </summary>
    [HttpGet("{driverId:guid}/shift-stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetShiftStatistics(
        Guid driverId,
        [FromQuery] DateTime? shiftStartUtc,
        [FromQuery] DateTime? shiftEndUtc)
    {
        var query = new GetShiftStatisticsQuery(driverId, shiftStartUtc, shiftEndUtc);
        var result = await _sender.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Updates driver availability status and location coordinates.
    /// </summary>
    [HttpPut("{driverId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid driverId, [FromBody] UpdateDriverStatusRequest request)
    {
        var command = new UpdateDriverStatusCommand(driverId, request.Status, request.CurrentLat, request.CurrentLng);
        var result = await _sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Synchronizes batched offline trip data for a driver.
    /// </summary>
    [HttpPost("sync-offline")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SyncOffline([FromBody] SyncOfflineDataCommand command)
    {
        var result = await _sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}

public sealed record UpdateDriverStatusRequest(string Status, double CurrentLat, double CurrentLng);
