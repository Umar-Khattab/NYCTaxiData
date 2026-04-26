using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Features.AI.DTOs;
using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IAiPredictionService"/> that communicates
/// with the Python FastAPI ML service via HTTP.
/// </summary>
public class AiPredictionService : IAiPredictionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiPredictionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiPredictionService"/> class.
    /// </summary>
    public AiPredictionService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AiPredictionService> logger)
    {
        var _ = _httpClient = httpClientFactory.CreateClient("MlService");
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<BatchPredictionResponse<Demand15MinResult>> PredictDemand15MinAsync(
        List<Demand15MinInput> zones, bool roundToInt, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.PostAsJsonAsync("/predict/demand_15min", new { zones, round_to_int = roundToInt }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BatchPredictionResponse<Demand15MinResult>>(ct);
        stopwatch.Stop();
        return result! with { Metadata = result.Metadata with { InferenceTimeMs = stopwatch.ElapsedMilliseconds } };
    }

    /// <inheritdoc />
    public async Task<BatchPredictionResponse<Demand6hResult>> PredictDemand6hAsync(
        List<Demand6hInput> zones, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.PostAsJsonAsync("/predict/demand_6h", new { zones }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BatchPredictionResponse<Demand6hResult>>(ct);
        stopwatch.Stop();
        return result! with { Metadata = result.Metadata with { InferenceTimeMs = stopwatch.ElapsedMilliseconds } };
    }

    /// <inheritdoc />
    public async Task<BatchPredictionResponse<ETAResult>> PredictETAAsync(
        List<ETAInput> routes, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.PostAsJsonAsync("/predict/eta", new { routes }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BatchPredictionResponse<ETAResult>>(ct);
        stopwatch.Stop();
        return result! with { Metadata = result.Metadata with { InferenceTimeMs = stopwatch.ElapsedMilliseconds } };
    }

    /// <inheritdoc />
    public async Task<BatchPredictionResponse<RevenueResult>> PredictRevenueAsync(
        List<RevenueInput> zones, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.PostAsJsonAsync("/predict/revenue", new { zones }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BatchPredictionResponse<RevenueResult>>(ct);
        stopwatch.Stop();
        return result! with { Metadata = result.Metadata with { InferenceTimeMs = stopwatch.ElapsedMilliseconds } };
    }

    /// <inheritdoc />
    public async Task<BatchPredictionResponse<StockOutResult>> PredictStockOutAsync(
        List<StockOutInput> zones, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.PostAsJsonAsync("/predict/stockout", new { zones }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<BatchPredictionResponse<StockOutResult>>(ct);
        stopwatch.Stop();
        return result! with { Metadata = result.Metadata with { InferenceTimeMs = stopwatch.ElapsedMilliseconds } };
    }

    /// <inheritdoc />
    public async Task<List<ProfitZoneResult>> RankZonesByProfitAsync(
        List<int> zoneIds, int currentHour, int dayOfWeek, bool considerStockOutRisk, int? topK, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/predict/profit_zones", new
        {
            zone_ids = zoneIds,
            current_hour = currentHour,
            day_of_week = dayOfWeek,
            consider_stockout_risk = considerStockOutRisk,
            top_k = topK
        }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ProfitZoneResult>>(ct);
        return result ?? new List<ProfitZoneResult>();
    }

    /// <inheritdoc />
    public async Task<CausalImpactResult> EstimateCausalImpactAsync(
        int zoneId, DateTime eventDate, string treatmentType, double baselineDemand, DateTime? baselineDate, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/predict/causal_impact", new
        {
            zone_id = zoneId,
            event_date = eventDate,
            treatment_type = treatmentType,
            baseline_demand = baselineDemand,
            baseline_date = baselineDate
        }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CausalImpactResult>(ct);
        return result!;
    }

    /// <inheritdoc />
    public async Task<RepositioningPlan> OptimizeRepositioningAsync(
        DateTime timeWindow, List<ZoneSupplyState> zoneStates, OptimizationConstraints? constraints, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/optimize/repositioning", new
        {
            time_window = timeWindow,
            zone_states = zoneStates,
            constraints
        }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RepositioningPlan>(ct);
        return result!;
    }

    /// <inheritdoc />
    public async Task<SimulationJobResponse> StartSimulationAsync(
        DateTime baseScenarioDate, int additionalVehicles, DeploymentStrategy strategy,
        int simulationDurationHours, double operationalCostPerVehiclePerDay,
        List<int>? targetZones, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/simulate/fleet_expansion", new
        {
            base_scenario_date = baseScenarioDate,
            additional_vehicles = additionalVehicles,
            strategy = strategy.ToString(),
            simulation_duration_hours = simulationDurationHours,
            operational_cost_per_vehicle_per_day = operationalCostPerVehiclePerDay,
            target_zones = targetZones
        }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SimulationJobResponse>(ct);
        return result!;
    }

    /// <inheritdoc />
    public async Task<SimulationResult?> GetSimulationResultAsync(string simulationId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/simulate/{simulationId}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SimulationResult>(ct);
    }
}
