namespace NYCTaxiData.Domain.Enums;

/// <summary>
/// Defines strategies for deploying additional vehicles in fleet expansion simulations.
/// </summary>
public enum DeploymentStrategy
{
    DemandBased,
    Uniform,
    ProfitBased,
    StockOutBased
}
