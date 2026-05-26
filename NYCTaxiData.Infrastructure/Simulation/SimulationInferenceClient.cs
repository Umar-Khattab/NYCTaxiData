using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Simulation.Models;

namespace NYCTaxiData.Infrastructure.Simulation;

public sealed class SimulationInferenceClient
{
    private readonly IAiPredictionService _aiPredictionService;
    private readonly ILogger<SimulationInferenceClient> _logger;

    public SimulationInferenceClient(IAiPredictionService aiPredictionService, ILogger<SimulationInferenceClient> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    public async Task<SimulationPredictionSet> PredictAsync(
        IReadOnlyList<SimulationZoneFeatures> features,
        CancellationToken ct)
    {
        var predictions = new SimulationPredictionSet();
        var demandInputs = features.Select(feature => feature.DemandInput).ToList();
        var revenueInputs = features.Select(feature => feature.RevenueInput).ToList();
        var stockInputs = features.Select(feature => feature.StockOutInput).ToList();
        var etaInputs = features.Select(feature => feature.EtaInput).ToList();

        try
        {
            var demandResults = await _aiPredictionService.PredictDemand6hAsync(demandInputs, ct);
            foreach (var result in demandResults)
            {
                predictions.DemandByZone[result.ZoneId] = result.P50;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Demand prediction failed; using fallback values.");
            foreach (var feature in features)
            {
                predictions.DemandByZone[feature.ZoneId] = feature.DemandInput.PickupCount;
            }
        }

        try
        {
            var revenueResults = await _aiPredictionService.PredictRevenueAsync(revenueInputs, ct);
            foreach (var result in revenueResults)
            {
                predictions.RevenueByZone[result.ZoneId] = result.ExpectedTotalRevenue;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Revenue prediction failed; using fallback values.");
            foreach (var feature in features)
            {
                predictions.RevenueByZone[feature.ZoneId] = feature.RevenueInput.AvgFare * feature.DemandInput.PickupCount;
            }
        }

        try
        {
            var stockResults = await _aiPredictionService.PredictStockOutAsync(stockInputs, ct);
            foreach (var result in stockResults)
            {
                predictions.StockoutRiskByZone[result.ZoneId] = result.Probability;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stockout prediction failed; using fallback values.");
            foreach (var feature in features)
            {
                var risk = Math.Clamp(feature.StockOutInput.PickupCount / 100, 0.05, 0.95);
                predictions.StockoutRiskByZone[feature.ZoneId] = risk;
            }
        }

        try
        {
            var etaResults = await _aiPredictionService.PredictETAAsync(etaInputs, ct);
            foreach (var result in etaResults)
            {
                predictions.EtaMinutesByZone[result.PickupZoneId] = result.P50Minutes;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ETA prediction failed; using fallback values.");
            foreach (var feature in features)
            {
                predictions.EtaMinutesByZone[feature.ZoneId] = (double)(feature.EtaInput.DurationSec / 60m);
            }
        }

        return predictions;
    }
}
