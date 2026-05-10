namespace NYCTaxiData.Domain.Enums;

/// <summary>
/// Represents the current status of a simulation job.
/// </summary>
public enum SimulationStatus
{
    Queued,
    Running,
    Completed,
    Failed
}
