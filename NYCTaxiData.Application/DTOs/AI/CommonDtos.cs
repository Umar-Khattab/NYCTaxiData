using NYCTaxiData.Domain.Enums;
using System.Text.Json.Serialization;

namespace NYCTaxiData.Application.DTOs.AI;

/// <summary>
/// Standard wrapper for batch prediction responses returned by the ML service.
/// </summary>
/// <typeparam name="T">Type of the individual prediction result.</typeparam>
public record BatchPredictionResponse<T>(
    string RequestId,
    DateTime PredictedAt,
    string ModelVersion,
    int Count,
    List<T> Results,
    PredictionMetadata Metadata
);

/// <summary>
/// Metadata about the prediction model and inference performance.
/// </summary>
public record PredictionMetadata(
    string ModelName,
    string ModelVersion,
    long InferenceTimeMs,
    int InputCount
);

/// <summary>
/// A complete repositioning plan with assignments and zone summaries.
/// </summary>
public record RepositioningPlan(
    [property: JsonPropertyName("plan_id")] string PlanId,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("valid_until")] DateTime ValidUntil,
    [property: JsonPropertyName("total_moves")] int TotalMoves,
    [property: JsonPropertyName("vehicles_moved")] int VehiclesMoved,
    [property: JsonPropertyName("estimated_relocation_cost")] double EstimatedRelocationCost,
    [property: JsonPropertyName("estimated_revenue_gain")] double EstimatedRevenueGain,
    [property: JsonPropertyName("net_profit_impact")] double NetProfitImpact,
    [property: JsonPropertyName("assignments")] List<RepositionAssignment> Assignments,
    [property: JsonPropertyName("zone_summaries")] List<ZonePlanSummary> ZoneSummaries
);

/// <summary>
/// Response returned when a fleet expansion simulation job is started.
/// </summary>
public record SimulationJobResponse(
    string SimulationId,
    SimulationStatus Status,
    DateTime CreatedAt,
    string? ResultUrl
);

/// <summary>
/// Full result of a completed fleet expansion simulation.
/// </summary>
public record SimulationResult(
    string SimulationId,
    SimulationStatus Status,
    DateTime CompletedAt,
    SimulationMetrics BaselineMetrics,
    SimulationMetrics SimulatedMetrics,
    FinancialImpact FinancialImpact,
    List<ZoneSimulationResult> ZoneBreakdown,
    SimulationRecommendation Recommendation
);
