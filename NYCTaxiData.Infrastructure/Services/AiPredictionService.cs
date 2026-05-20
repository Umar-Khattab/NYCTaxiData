using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Domain.DTOs;
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
    public AiPredictionService(HttpClient httpClient, ILogger<AiPredictionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<Demand15MinResult>> PredictDemand15MinAsync(
        List<Demand15MinInput> zones, bool roundToInt, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/predict/demand_15min", new { zones, round_to_int = roundToInt }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Demand15MinResult>>(ct);
        return result ?? new List<Demand15MinResult>();
    }

    /// <inheritdoc />
    public async Task<List<Demand6hResult>> PredictDemand6hAsync(
        List<Demand6hInput> zones, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.PostAsJsonAsync("/predict/demand_6h", new { zones }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<Demand6hResult>>(ct);
        stopwatch.Stop();
        return result ?? new List<Demand6hResult>();
    }

    /// <inheritdoc />
    public async Task<List<ETAResult>> PredictETAAsync(
        List<ETAInput> routes, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        // الكود بتاعك سليم بس محتاج الـ BaseAddress يكون محقون صح
        var response = await _httpClient.PostAsJsonAsync("predict/eta", new { routes }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ETAResult>>(ct);
        stopwatch.Stop();
        return result ?? new List<ETAResult>();
    }

    /// <inheritdoc />
    public async Task<List<RevenueResult>> PredictRevenueAsync(
        List<RevenueInput> zones, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.PostAsJsonAsync("/predict/revenue", new { zones }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<RevenueResult>>(ct);
        stopwatch.Stop();
        return result ?? new List<RevenueResult>();
    }

    /// <inheritdoc />
    public async Task<List<StockOutResult>> PredictStockOutAsync(
        List<StockOutInput> zones, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.PostAsJsonAsync("/predict/stockout", new { zones }, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<StockOutResult>>(ct);
        stopwatch.Stop();
        return result ?? new List<StockOutResult>();
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
}
