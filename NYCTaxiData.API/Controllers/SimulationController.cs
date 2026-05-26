using Microsoft.AspNetCore.Mvc;
using NYCTaxiData.API.Contracts;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.API.Controllers;

[ApiController]
[Route("api/v1/simulation")]
public sealed class SimulationController : ControllerBase
{
    private readonly ISimulationOrchestrator _orchestrator;

    public SimulationController(ISimulationOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<SimulationStatusResponse>>> Start(
        [FromBody] SimulationStartRequest request,
        CancellationToken ct)
    {
        var status = await _orchestrator.StartAsync(request, ct);
        return Ok(ApiResponse<SimulationStatusResponse>.Success(status, "Simulation started"));
    }

    [HttpPost("pause")]
    public async Task<ActionResult<ApiResponse<SimulationStatusResponse>>> Pause(CancellationToken ct)
    {
        var status = await _orchestrator.PauseAsync(ct);
        return Ok(ApiResponse<SimulationStatusResponse>.Success(status, "Simulation paused"));
    }

    [HttpPost("resume")]
    public async Task<ActionResult<ApiResponse<SimulationStatusResponse>>> Resume(CancellationToken ct)
    {
        var status = await _orchestrator.ResumeAsync(ct);
        return Ok(ApiResponse<SimulationStatusResponse>.Success(status, "Simulation resumed"));
    }

    [HttpPost("stop")]
    public async Task<ActionResult<ApiResponse<SimulationStatusResponse>>> Stop(CancellationToken ct)
    {
        var status = await _orchestrator.StopAsync(ct);
        return Ok(ApiResponse<SimulationStatusResponse>.Success(status, "Simulation stopped"));
    }

    [HttpPost("speed")]
    public async Task<ActionResult<ApiResponse<SimulationStatusResponse>>> SetSpeed(
        [FromBody] SimulationSpeedRequest request,
        CancellationToken ct)
    {
        var status = await _orchestrator.SetSpeedAsync(request.SpeedFactor, ct);
        return Ok(ApiResponse<SimulationStatusResponse>.Success(status, "Speed updated"));
    }

    [HttpGet("status")]
    public ActionResult<ApiResponse<SimulationStatusResponse>> Status()
    {
        var status = _orchestrator.GetStatus();
        return Ok(ApiResponse<SimulationStatusResponse>.Success(status));
    }

    [HttpGet("playback")]
    public ActionResult<ApiResponse<SimulationPlaybackChunk>> Playback(
        [FromQuery] int startHour = 0,
        [FromQuery] int endHour = 23)
    {
        var playback = _orchestrator.GetPlayback(startHour, endHour);
        return Ok(ApiResponse<SimulationPlaybackChunk>.Success(playback));
    }

    [HttpGet("zones")]
    public ActionResult<ApiResponse<IReadOnlyList<int>>> Zones()
    {
        var zones = _orchestrator.GetZoneIds();
        return Ok(ApiResponse<IReadOnlyList<int>>.Success(zones));
    }

    [HttpGet("zones/{zoneId:int}/history")]
    public ActionResult<ApiResponse<ZoneHistoryResponse>> ZoneHistory([FromRoute] int zoneId)
    {
        var history = _orchestrator.GetZoneHistory(zoneId);
        return Ok(ApiResponse<ZoneHistoryResponse>.Success(history));
    }

    [HttpGet("zones/compare")]
    public ActionResult<ApiResponse<IReadOnlyList<ZoneHistoryResponse>>> CompareZones(
        [FromQuery] int zoneA,
        [FromQuery] int zoneB)
    {
        var history = new List<ZoneHistoryResponse>
        {
            _orchestrator.GetZoneHistory(zoneA),
            _orchestrator.GetZoneHistory(zoneB)
        };
        return Ok(ApiResponse<IReadOnlyList<ZoneHistoryResponse>>.Success(history));
    }
}

public record SimulationSpeedRequest(double SpeedFactor);
