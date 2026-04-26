namespace NYCTaxiData.Domain.Enums;

/// <summary>
/// Categorizes the recommendation outcome of a simulation analysis.
/// </summary>
public enum RecommendationType
{
    PositiveROI,
    NegativeROI,
    Marginal,
    NeedsMoreData
}
