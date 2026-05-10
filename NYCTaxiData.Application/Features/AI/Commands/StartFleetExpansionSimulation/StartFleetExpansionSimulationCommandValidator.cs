using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.StartFleetExpansionSimulation;

/// <summary>
/// Validator for <see cref="StartFleetExpansionSimulationCommand"/>.
/// </summary>
public class StartFleetExpansionSimulationCommandValidator : AbstractValidator<StartFleetExpansionSimulationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartFleetExpansionSimulationCommandValidator"/> class.
    /// </summary>
    public StartFleetExpansionSimulationCommandValidator()
    {
        RuleFor(x => x.BaseScenarioDate)
            .NotEmpty();

        RuleFor(x => x.AdditionalVehicles)
            .InclusiveBetween(1, 10000);

        RuleFor(x => x.SimulationDurationHours)
            .InclusiveBetween(1, 720);

        RuleFor(x => x.OperationalCostPerVehiclePerDay)
            .GreaterThanOrEqualTo(0);

        RuleForEach(x => x.TargetZones)
            .InclusiveBetween(1, 265)
            .When(x => x.TargetZones is not null);
    }
}
