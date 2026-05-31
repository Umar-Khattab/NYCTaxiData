using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.DTOs;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Application.Common.Interfaces;

/// <summary>
/// Service interface for AI/ML prediction operations via the external ML service.
/// </summary>
public interface IAiPredictionService
{
    // ========================================================================
    // EXISTING methods (keep them - do not remove)
    // ========================================================================
    // Task<...> GetDemandForecastAsync(...);
    // Task<...> GetDispatchRecommendationAsync(...);
    // Task<...> GetOptimalDriverScheduleAsync(...);
    // Task<...> GetExplainableAiInsightAsync(...);
    // Task<...> ProcessVoiceAssistantQueryAsync(...);
    // Task<...> RunOperationalSimulationAsync(...);
    // Task<...> RunStrategicSimulationAsync(...);
    // Task<...> TriggerModelRetrainingAsync(...);

    // ========================================================================
    // NEW methods to add for AI Module Endpoints
    // ========================================================================

    /// <summary>
    /// Predicts 15-minute demand for the specified zones.
    /// </summary>
    Task<List<Demand15MinResult>> PredictDemand15MinAsync(
        List<Demand15MinInput> zones, bool roundToInt, CancellationToken ct = default);

    /// <summary>
    /// Predicts 6-hour demand for the specified zones.
    /// </summary>
    Task<List<Demand6hResult>> PredictDemand6hAsync(
        List<Demand6hInput> zones, CancellationToken ct = default);

    /// <summary>
    /// Predicts ETA for the specified zone pairs.
    /// </summary>
    Task<List<ETAResult>> PredictETAAsync(
        List<ETAInput> routes, CancellationToken ct = default);

    /// <summary>
    /// Predicts revenue for the specified zones.
    /// </summary>
    Task<List<RevenueResult>> PredictRevenueAsync(
        List<RevenueInput> zones, CancellationToken ct = default);

    /// <summary>
    /// Predicts stock-out probability for the specified zones.
    /// </summary>
    Task<List<StockOutResult>> PredictStockOutAsync(
        List<StockOutInput> zones, CancellationToken ct = default);

    /// <summary>
    /// Optimizes vehicle repositioning across zones.
    /// </summary>
    Task<RepositioningPlan> OptimizeRepositioningAsync(
        DateTime timeWindow, List<ZoneSupplyState> zoneStates, OptimizationConstraints? constraints, CancellationToken ct = default);

    /// <summary>
    /// Optimizes vehicle repositioning to maximize profit in the next 6 hours.
    /// </summary>
    Task<ProfitMaximizationResult> MaximizeProfitAsync(
        List<ProfitMaximizationInput> zones, CancellationToken ct = default);
}
