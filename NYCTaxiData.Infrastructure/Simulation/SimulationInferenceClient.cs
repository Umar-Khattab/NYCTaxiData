using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Simulation.Models;
using NYCTaxiData.Application.DTOs.AI;

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

        // 🚀 Execute all 4 predictions concurrently to eliminate sequential HTTP wait time
        var demandTask = _aiPredictionService.PredictDemand6hAsync(demandInputs, ct);
        var revenueTask = _aiPredictionService.PredictRevenueAsync(revenueInputs, ct);
        var stockTask = _aiPredictionService.PredictStockOutAsync(stockInputs, ct);
        var etaTask = _aiPredictionService.PredictETAAsync(etaInputs, ct);

        try
        {
            await Task.WhenAll(demandTask, revenueTask, stockTask, etaTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "One or more prediction models failed during concurrent simulation step.");
        }

        // 📊 Process Demand Prediction Task
        if (demandTask.Status == TaskStatus.RanToCompletion)
        {
            var demandResults = demandTask.Result;
            foreach (var result in demandResults)
            {
                predictions.DemandByZone[result.ZoneId] = result.P50;
            }
        }
        else
        {
            var innerEx = demandTask.Exception?.InnerException ?? new Exception("Demand prediction task failed");
            _logger.LogWarning(innerEx, "Demand prediction failed; using historical fallback values.");
            foreach (var feature in features)
            {
                predictions.DemandByZone[feature.ZoneId] = feature.DemandInput.PickupCount;
            }
        }

        // 📊 Process Revenue Prediction Task
        if (revenueTask.Status == TaskStatus.RanToCompletion)
        {
            var revenueResults = revenueTask.Result;
            foreach (var result in revenueResults)
            {
                predictions.RevenueByZone[result.ZoneId] = result.ExpectedTotalRevenue;
            }
        }
        else
        {
            var innerEx = revenueTask.Exception?.InnerException ?? new Exception("Revenue prediction task failed");
            _logger.LogWarning(innerEx, "Revenue prediction failed; using historical fallback values.");
            foreach (var feature in features)
            {
                predictions.RevenueByZone[feature.ZoneId] = feature.RevenueInput.AvgFare * feature.DemandInput.PickupCount;
            }
        }

        // 📊 Process Stockout Prediction Task
        if (stockTask.Status == TaskStatus.RanToCompletion)
        {
            var stockResults = stockTask.Result;
            foreach (var result in stockResults)
            {
                predictions.StockoutRiskByZone[result.ZoneId] = result.Probability;
            }
        }
        else
        {
            var innerEx = stockTask.Exception?.InnerException ?? new Exception("Stockout prediction task failed");
            _logger.LogWarning(innerEx, "Stockout prediction failed; using historical fallback values.");
            foreach (var feature in features)
            {
                var risk = Math.Clamp(feature.StockOutInput.PickupCount / 100, 0.05, 0.95);
                predictions.StockoutRiskByZone[feature.ZoneId] = risk;
            }
        }

        // 📊 Process ETA Prediction Task
        if (etaTask.Status == TaskStatus.RanToCompletion)
        {
            var etaResults = etaTask.Result;
            foreach (var result in etaResults)
            {
                predictions.EtaMinutesByZone[result.PickupZoneId] = result.P50Minutes;
            }
        }
        else
        {
            var innerEx = etaTask.Exception?.InnerException ?? new Exception("ETA prediction task failed");
            _logger.LogWarning(innerEx, "ETA prediction failed; using historical fallback values.");
            foreach (var feature in features)
            {
                predictions.EtaMinutesByZone[feature.ZoneId] = (double)(feature.EtaInput.DurationSec / 60m);
            }
        }

        return predictions;
    }
}
