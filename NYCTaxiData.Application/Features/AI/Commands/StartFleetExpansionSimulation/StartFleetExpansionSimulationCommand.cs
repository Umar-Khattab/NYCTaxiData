using System.ComponentModel.DataAnnotations;
using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.Features.AI.DTOs;
using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Application.Features.AI.Commands.StartFleetExpansionSimulation;

/// <summary>
/// Command to start a fleet expansion simulation job.
/// This command is idempotent to prevent duplicate simulation runs.
/// </summary>
public record StartFleetExpansionSimulationCommand(
    DateTime BaseScenarioDate,
    [Range(1, 10000)] int AdditionalVehicles,
    DeploymentStrategy Strategy,
    [Range(1, 720)] int SimulationDurationHours = 168,
    double OperationalCostPerVehiclePerDay = 85.0,
    List<int>? TargetZones = null
) : IRequest<Result<SimulationJobResponse>>, IIdempotentCommand
{
    /// <inheritdoc />
    public string IdempotencyKey => $"fleet-expansion-{BaseScenarioDate:yyyyMMdd}-{AdditionalVehicles}-{Strategy}";
}
