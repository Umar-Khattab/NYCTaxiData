using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Common.Interfaces;

/// <summary>
/// Service interface for retrieving engineered ML features internally from the AI database.
/// Decouples the frontend from climate, lags, and other engineered feature shapes.
/// </summary>
public interface IAiFeatureProvider
{
    /// <summary>
    /// Gets 15-minute demand prediction features for the specified zones and target time.
    /// </summary>
    Task<List<Demand15MinInput>> GetDemand15MinFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default);

    /// <summary>
    /// Gets 6-hour demand prediction features for the specified zones and target time.
    /// </summary>
    Task<List<Demand6hInput>> GetDemand6hFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default);

    /// <summary>
    /// Gets ETA prediction features for the specified route pairs and target times.
    /// </summary>
    Task<List<ETAInput>> GetEtaFeaturesAsync(List<RouteRequest> routes, CancellationToken ct = default);

    /// <summary>
    /// Gets revenue prediction features for the specified zones and target time.
    /// </summary>
    Task<List<RevenueInput>> GetRevenueFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default);

    /// <summary>
    /// Gets stock-out prediction features for the specified zones and target time.
    /// </summary>
    Task<List<StockOutInput>> GetStockOutFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default);
}
