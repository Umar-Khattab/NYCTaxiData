using MediatR;
using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Contracts;
using NYCTaxiData.Application.Features.Trips.Commands.EndTrip;
using NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch;
using NYCTaxiData.Application.Features.Trips.Commands.StartTrip; 
using NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;

namespace NYCTaxiData.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TripsController(IMediator _mediator) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IActionResult> StartTrip([FromBody] StartTripCommand command)
    {
        var result = await _mediator.Send(command);
        // استخدام CreatedAtAction أفضل هندسياً للـ POST
        return StatusCode(201, new APIResponse<object> { IsSuccess = true, Message = "Trip started", Data = result });
    }

    [HttpPost("end")]
    public async Task<IActionResult> EndTrip([FromBody] EndTripCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new APIResponse<object> { IsSuccess = true, Message = "Trip ended", Data = result });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetTripHistory([FromQuery] Guid driverId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    { 
        var result = await _mediator.Send(new GetTripHistoryQuery(driverId, page, limit));
        return Ok(new APIResponse<object> { IsSuccess = true, Message = "History retrieved", Data = result });
    }

    [HttpGet("dispatch/feed")]
    public async Task<IActionResult> GetLiveDispatchFeed([FromQuery] int limit = 5, [FromQuery] int minutesWindow = 60)
    { 
        var result = await _mediator.Send(new GetLiveDispatchFeedQuery(limit, minutesWindow));
        return Ok(new APIResponse<object> { IsSuccess = true, Message = "Feed retrieved", Data = result });
    }

    [HttpPost("dispatch/manual")]
    public async Task<IActionResult> ManualDispatch([FromBody] ManualDispatchCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new APIResponse<object> { IsSuccess = true, Message = "Manual dispatch created", Data = result });
    }
}