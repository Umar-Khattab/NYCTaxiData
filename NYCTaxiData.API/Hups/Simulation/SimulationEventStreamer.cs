using Microsoft.AspNetCore.SignalR;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.DTOs.Simulation;

namespace NYCTaxiData.API.Hups.Simulation;

public sealed class SimulationEventStreamer : ISimulationEventStreamer
{
    private readonly IHubContext<SimulationHub> _hubContext;

    public SimulationEventStreamer(IHubContext<SimulationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task BroadcastTickAsync(SimulationTick tick, CancellationToken ct = default)
        => _hubContext.Clients.All.SendAsync("SimulationTick", tick, ct);

    public Task BroadcastStatusAsync(SimulationStatusResponse status, CancellationToken ct = default)
        => _hubContext.Clients.All.SendAsync("SimulationStatus", status, ct);
}
