using System;

namespace NYCTaxiData.Application.Common.Interfaces;

/// <summary>
/// Service interface responsible for resolving and aligning absolute date contexts
/// to the valid, fully populated historical feature snapshot inside the AI database.
/// Decouples the prediction pipeline from DateTime.UtcNow/Now.
/// </summary>
public interface IAiTemporalResolver
{
    /// <summary>
    /// Aligns targetTime to a corresponding, statistically equivalent date within the valid AI snapshot range,
    /// cyclically mapping months and preserving hour, minute, and Day of Week bounds.
    /// </summary>
    /// <param name="targetTime">The incoming date context to resolve.</param>
    /// <returns>A fully resolved, snapshot-compatible DateTime.</returns>
    DateTime ResolveTemporalContext(DateTime targetTime);

    /// <summary>
    /// Gets the latest available snapshot date limit inside the database.
    /// </summary>
    DateTime GetLatestSnapshotDate();
}
