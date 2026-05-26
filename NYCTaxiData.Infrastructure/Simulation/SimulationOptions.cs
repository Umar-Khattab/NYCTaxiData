namespace NYCTaxiData.Infrastructure.Simulation;

public sealed class SimulationOptions
{
    public int DefaultDurationHours { get; set; } = 24;
    public double DefaultSpeedFactor { get; set; } = 60;
    public int DefaultDriverCount { get; set; } = 300;
    public int DefaultZoneCount { get; set; } = 30;
    public int MaxRelocationsPerHour { get; set; } = 25;
    public string? FeatureDataPath { get; set; }
}
