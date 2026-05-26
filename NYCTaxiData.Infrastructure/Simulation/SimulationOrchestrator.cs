using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Simulation.Models;
using NYCTaxiData.Domain.DTOs;
using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Infrastructure.Simulation;

public sealed class SimulationOrchestrator : ISimulationOrchestrator
{
    private readonly ISimulationFeatureLoader _featureLoader;
    private readonly ISimulationStateManager _stateManager;
    private readonly ISimulationRuleEngine _ruleEngine;
    private readonly ISimulationResultStore _resultStore;
    private readonly ISimulationEventStreamer _eventStreamer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SimulationOrchestrator> _logger;
    private readonly SimulationOptions _options;
    private readonly object _sync = new();
    private SimulationState? _state;
    private CancellationTokenSource? _cts;
    private Task? _runTask;

    public SimulationOrchestrator(
        ISimulationFeatureLoader featureLoader,
        ISimulationStateManager stateManager,
        ISimulationRuleEngine ruleEngine,
        ISimulationResultStore resultStore,
        ISimulationEventStreamer eventStreamer,
        IServiceScopeFactory scopeFactory,
        IOptions<SimulationOptions> options,
        ILogger<SimulationOrchestrator> logger)
    {
        _featureLoader = featureLoader;
        _stateManager = stateManager;
        _ruleEngine = ruleEngine;
        _resultStore = resultStore;
        _eventStreamer = eventStreamer;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<SimulationStatusResponse> StartAsync(SimulationStartRequest request, CancellationToken ct = default)
    {
        var normalized = NormalizeRequest(request);
        var features = await _featureLoader.LoadHourlyFeaturesAsync(normalized.StartTime, normalized.ZoneCount, ct);
        var state = _stateManager.InitializeState(normalized, features);

        lock (_sync)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            _state = state;
            _state.Status = SimulationStatus.Running;
            _resultStore.Reset(state.SimulationId);
            _runTask = Task.Run(() => RunSimulationLoopAsync(state, _cts.Token), CancellationToken.None);
        }

        var status = BuildStatus(state);
        await _eventStreamer.BroadcastStatusAsync(status, ct);
        return status;
    }

    public Task<SimulationStatusResponse> PauseAsync(CancellationToken ct = default)
    {
        var state = GetStateOrThrow();
        state.IsPaused = true;
        var status = BuildStatus(state);
        return BroadcastStatusAsync(status, ct);
    }

    public Task<SimulationStatusResponse> ResumeAsync(CancellationToken ct = default)
    {
        var state = GetStateOrThrow();
        state.IsPaused = false;
        var status = BuildStatus(state);
        return BroadcastStatusAsync(status, ct);
    }

    public Task<SimulationStatusResponse> StopAsync(CancellationToken ct = default)
    {
        SimulationState? state;
        lock (_sync)
        {
            _cts?.Cancel();
            state = _state;
        }

        if (state is null)
        {
            return Task.FromResult(GetStatus());
        }

        state.Status = SimulationStatus.Stopped;
        state.IsPaused = false;
        var status = BuildStatus(state);
        return BroadcastStatusAsync(status, ct);
    }

    public Task<SimulationStatusResponse> SetSpeedAsync(double speedFactor, CancellationToken ct = default)
    {
        var state = GetStateOrThrow();
        state.SpeedFactor = Math.Clamp(speedFactor, 1, 200);
        var status = BuildStatus(state);
        return BroadcastStatusAsync(status, ct);
    }

    public SimulationStatusResponse GetStatus()
    {
        var state = _state;
        return state is null
            ? new SimulationStatusResponse(Guid.Empty.ToString(), SimulationStatus.Queued, null, 0, _options.DefaultSpeedFactor, false)
            : BuildStatus(state);
    }

    public SimulationTick? GetLatestTick() => _resultStore.GetLatestTick();

    public ZoneHistoryResponse GetZoneHistory(int zoneId) => _resultStore.GetZoneHistory(zoneId);

    public SimulationPlaybackChunk GetPlayback(int startHour, int endHour) => _resultStore.GetPlayback(startHour, endHour);

    public IReadOnlyList<int> GetZoneIds()
    {
        var state = _state;
        return state is null || state.Zones.Count == 0
            ? _resultStore.GetZoneIds()
            : state.Zones.Keys.OrderBy(id => id).ToList();
    }

    private async Task RunSimulationLoopAsync(SimulationState state, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && state.CurrentHourIndex < state.DurationHours)
        {
            if (state.IsPaused)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250), ct);
                continue;
            }

            var stepStart = DateTime.UtcNow;
            try
            {
                var features = await _featureLoader.LoadHourlyFeaturesAsync(state.CurrentTime, state.Zones.Count, ct);
                using var scope = _scopeFactory.CreateScope();
                var inferenceClient = ActivatorUtilities.CreateInstance<SimulationInferenceClient>(
                    scope.ServiceProvider);
                var predictions = await inferenceClient.PredictAsync(features, ct);
                _stateManager.ApplyStep(state, predictions);
                var relocations = _ruleEngine.ComputeRelocations(state);
                _stateManager.ApplyRelocations(state, relocations);
                var tick = _stateManager.BuildTick(state);
                _resultStore.AppendTick(tick);
                await _eventStreamer.BroadcastTickAsync(tick, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Simulation step failed at hour {Hour}", state.CurrentHourIndex);
                state.Status = SimulationStatus.Failed;
                await _eventStreamer.BroadcastStatusAsync(BuildStatus(state), ct);
                return;
            }

            state.CurrentHourIndex += 1;
            state.CurrentTime = state.CurrentTime.AddHours(1);

            var stepDuration = TimeSpan.FromSeconds(Math.Max(0.1, 3600.0 / state.SpeedFactor));
            var elapsed = DateTime.UtcNow - stepStart;
            var delay = stepDuration - elapsed;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, ct);
            }
        }

        if (ct.IsCancellationRequested || state.Status is SimulationStatus.Stopped or SimulationStatus.Failed)
        {
            return;
        }

        state.Status = SimulationStatus.Completed;
        await _eventStreamer.BroadcastStatusAsync(BuildStatus(state), ct);
    }

    private SimulationState GetStateOrThrow()
    {
        var state = _state;
        if (state is null)
        {
            throw new InvalidOperationException("Simulation has not been started.");
        }

        return state;
    }

    private SimulationStartRequest NormalizeRequest(SimulationStartRequest request)
    {
        var duration = request.DurationHours <= 0 ? _options.DefaultDurationHours : request.DurationHours;
        var speed = request.SpeedFactor <= 0 ? _options.DefaultSpeedFactor : request.SpeedFactor;
        var drivers = request.TotalDrivers <= 0 ? _options.DefaultDriverCount : request.TotalDrivers;
        var zones = request.ZoneCount <= 0 ? _options.DefaultZoneCount : request.ZoneCount;
        var startTime = request.StartTime == default ? DateTime.UtcNow.Date : request.StartTime;

        return new SimulationStartRequest(duration, speed, drivers, zones, startTime);
    }

    private SimulationStatusResponse BuildStatus(SimulationState state)
        => new(
            state.SimulationId.ToString(),
            state.Status,
            state.CurrentTime,
            state.CurrentHourIndex,
            state.SpeedFactor,
            state.IsPaused);

    private async Task<SimulationStatusResponse> BroadcastStatusAsync(SimulationStatusResponse status, CancellationToken ct)
    {
        await _eventStreamer.BroadcastStatusAsync(status, ct);
        return status;
    }
}
