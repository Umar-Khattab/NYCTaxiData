using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Domain.Enums;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace NYCTaxiData.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IAiPredictionService"/> that communicates
/// with the Python FastAPI ML service via HTTP.
/// </summary>
public class AiPredictionService : IAiPredictionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiPredictionService> _logger;
    private readonly string _endpoint = "api/optimize/repositioning";

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
        // 1. تعريف الـ options هنا داخل الميثود
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // يمنع الـ CamelCase ويحافظ على أسماء الـ Attributes
        };

        // أرسل الطلب
        var requestBody = new { rows = zones, round_to_int = roundToInt };

        // 2. استخدم JsonSerializer.Serialize لضمان الالتزام بالـ options
        var jsonString = JsonSerializer.Serialize(requestBody, options);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/predict/demand_15min", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            Console.WriteLine($"[ML SERVICE ERROR 422] Content: {errorContent}");
            throw new Exception($"ML Prediction failed with 422: {errorContent}");
        }

        // 3. استخدام الـ options هنا في القراءة
        var result = await response.Content.ReadFromJsonAsync<List<Demand15MinResult>>(options, ct);

        return result ?? new List<Demand15MinResult>();
    }

    /// <inheritdoc />
    public async Task<List<Demand6hResult>> PredictDemand6hAsync(List<Demand6hInput> zones, CancellationToken ct = default)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // يمنع الـ CamelCase
        };

        var requestBody = new { rows = zones };

        // تحويل البيانات يدوياً إلى JSON String
        var jsonString = JsonSerializer.Serialize(requestBody, options);
        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        // إرسال الـ Content مباشرة
        var response = await _httpClient.PostAsync("/predict/demand_6h", content, ct);

        var rawResponse = await response.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("ML Service Raw Response: {Raw}", rawResponse);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("ML Service Error 422: {Error}", errorContent);
            response.EnsureSuccessStatusCode();
        }  
        var result = await response.Content.ReadFromJsonAsync<List<Demand6hResult>>(options, ct);
        return result ?? new List<Demand6hResult>();
    }
    /// <inheritdoc />
    public async Task<List<ETAResult>> PredictETAAsync(List<ETAInput> routes, CancellationToken ct = default)
    {
        var requestBody = new { rows = routes };
        var response = await _httpClient.PostAsJsonAsync("predict/eta", requestBody, ct);
         
        var rawDataList = await response.Content.ReadFromJsonAsync<List<PredictionResponse>>(ct);
         
        return rawDataList.Select((item, index) => new ETAResult(
            routes[index].PickupZoneId,
            routes[index].DropoffZoneId,
            item.Predictions.P50Seconds,
            item.Predictions.P90Seconds
        )).ToList();
    }
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
    // C#
    public async Task<RepositioningPlan> OptimizeRepositioningAsync(
        DateTime timeWindow, List<ZoneSupplyState> zoneStates, OptimizationConstraints? constraints, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(_endpoint, new
        {
            time_window = timeWindow,
            zone_states = zoneStates,
            constraints
        }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("ML service returned {StatusCode} for repositioning: {Body}", response.StatusCode, body);
            // Throw a clearer exception (include status) that handler can interpret if needed
            throw new HttpRequestException($"ML service responded {response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<RepositioningPlan>(ct);
        return result ?? throw new InvalidOperationException("ML service returned no plan.");
    }
}
