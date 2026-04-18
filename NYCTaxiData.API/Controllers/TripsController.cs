using MediatR;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.Application.Features.Trips.Commands.EndTrip;
using NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch;
using NYCTaxiData.Application.Features.Trips.Commands.StartTrip;
using NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;

namespace NYCTaxiData.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ISender _sender;

    public TripsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Starts a new trip.
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartTrip([FromBody] StartTripCommand command)
    {
        var result = await _sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Ends an active trip.
    /// </summary>
    [HttpPost("end")]
    public async Task<IActionResult> EndTrip([FromBody] EndTripCommand command)
    {
        var result = await _sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves trip history based on query filters.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetTripHistory([FromQuery] GetTripHistoryQuery query)
    {
        var result = await _sender.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves the live dispatch feed.
    /// </summary>
    [HttpGet("live-dispatch")]
    public async Task<IActionResult> GetLiveDispatchFeed([FromQuery] GetLiveDispatchFeedQuery query)
    {
        var result = await _sender.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Creates a manual dispatch.
    /// </summary>
    [HttpPost("dispatch")]
    public async Task<IActionResult> ManualDispatch([FromBody] ManualDispatchCommand command)
    {
        var result = await _sender.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}