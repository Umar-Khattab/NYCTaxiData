using NYCTaxiData.Application.Simulation.Models;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Application.Common.Interfaces.Simulation;

public interface ISimulationOrchestrator
{
    Task<SimulationStatusResponse> StartAsync(SimulationStartRequest request, CancellationToken ct = default);
    Task<SimulationStatusResponse> PauseAsync(CancellationToken ct = default);
    Task<SimulationStatusResponse> ResumeAsync(CancellationToken ct = default);
    Task<SimulationStatusResponse> StopAsync(CancellationToken ct = default);
    Task<SimulationStatusResponse> SetSpeedAsync(double speedFactor, CancellationToken ct = default);
    SimulationStatusResponse GetStatus();
    SimulationTick? GetLatestTick();
    ZoneHistoryResponse GetZoneHistory(int zoneId);
    SimulationPlaybackChunk GetPlayback(int startHour, int endHour);
    IReadOnlyList<int> GetZoneIds();
}

public interface ISimulationFeatureLoader
{
    Task<IReadOnlyList<SimulationZoneFeatures>> LoadHourlyFeaturesAsync(
        DateTime simulatedTime,
        int zoneCount,
        CancellationToken ct = default);
}

public interface ISimulationRuleEngine
{
    IReadOnlyList<DriverRelocation> ComputeRelocations(SimulationState state);
}

public interface ISimulationStateManager
{
    SimulationState InitializeState(SimulationStartRequest request, IReadOnlyList<SimulationZoneFeatures> features);
    void ApplyStep(SimulationState state, SimulationPredictionSet predictions);
    SimulationTick BuildTick(SimulationState state);
    void ApplyRelocations(SimulationState state, IReadOnlyList<DriverRelocation> relocations);
}

public interface ISimulationResultStore
{
    void Reset(Guid simulationId);
    void AppendTick(SimulationTick tick);
    SimulationTick? GetLatestTick();
    ZoneHistoryResponse GetZoneHistory(int zoneId);
    SimulationPlaybackChunk GetPlayback(int startHour, int endHour);
    IReadOnlyList<int> GetZoneIds();
}

public interface ISimulationEventStreamer
{
    Task BroadcastTickAsync(SimulationTick tick, CancellationToken ct = default);
    Task BroadcastStatusAsync(SimulationStatusResponse status, CancellationToken ct = default);
}

public record DriverRelocation(int DriverId, int FromZoneId, int ToZoneId);
