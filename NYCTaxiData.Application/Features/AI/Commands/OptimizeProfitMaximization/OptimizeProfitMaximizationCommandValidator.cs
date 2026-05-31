using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.OptimizeProfitMaximization;

/// <summary>
/// Validator for <see cref="OptimizeProfitMaximizationCommand"/>.
/// </summary>
public class OptimizeProfitMaximizationCommandValidator : AbstractValidator<OptimizeProfitMaximizationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OptimizeProfitMaximizationCommandValidator"/> class.
    /// </summary>
    public OptimizeProfitMaximizationCommandValidator()
    {
        RuleFor(x => x.ZoneStates)
            .NotEmpty().WithMessage("At least one zone supply state features is required");

        RuleForEach(x => x.ZoneStates).ChildRules(state =>
        {
            state.RuleFor(s => s.ZoneId).InclusiveBetween(1, 265).WithMessage("ZoneId must be between 1 and 265");
            state.RuleFor(s => s.CurrentDrivers).GreaterThanOrEqualTo(0).WithMessage("CurrentDrivers must be greater than or equal to 0");
        });
    }
}
