using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.DTOs.Simulation;

namespace NYCTaxiData.API.Hups.Simulation;

[AllowAnonymous]
public sealed class SimulationHub : Hub
{
    private readonly ISimulationOrchestrator _orchestrator;

    public SimulationHub(ISimulationOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public Task<SimulationStatusResponse> StartSimulation(SimulationStartRequest request, CancellationToken ct)
        => _orchestrator.StartAsync(request, ct);

    public Task<SimulationStatusResponse> ControlSimulation(SimulationControlRequest request, CancellationToken ct)
    {
        return request.Action?.ToLowerInvariant() switch
        {
            "pause" => _orchestrator.PauseAsync(ct),
            "resume" => _orchestrator.ResumeAsync(ct),
            "stop" => _orchestrator.StopAsync(ct),
            "speed" when request.SpeedFactor.HasValue => _orchestrator.SetSpeedAsync(request.SpeedFactor.Value, ct),
            _ => Task.FromResult(_orchestrator.GetStatus())
        };
    }

    public Task<SimulationStatusResponse> GetStatus(CancellationToken ct)
        => Task.FromResult(_orchestrator.GetStatus());
}
