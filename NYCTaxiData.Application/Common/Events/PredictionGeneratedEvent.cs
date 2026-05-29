using MediatR;
using System.Collections.Generic;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Common.Events;

/// <summary>
/// Domain Event published when AI prediction forecasts are successfully generated.
/// This enables event-driven decoupling between the AI module and other modules like Zones.
/// </summary>
public record PredictionGeneratedEvent(
    string PredictionType,
    List<Demand15MinResult>? Demand15MinResults = null,
    List<Demand6hResult>? Demand6hResults = null,
    List<ETAResult>? EtaResults = null,
    List<RevenueResult>? RevenueResults = null,
    List<StockOutResult>? StockOutResults = null
) : INotification;
