using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.OptimizeRepositioning;

/// <summary>
/// Validator for <see cref="OptimizeRepositioningCommand"/>.
/// </summary>
public class OptimizeRepositioningCommandValidator : AbstractValidator<OptimizeRepositioningCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OptimizeRepositioningCommandValidator"/> class.
    /// </summary>
    public OptimizeRepositioningCommandValidator()
    {
        RuleFor(x => x.ZoneStates)
            .NotEmpty().WithMessage("At least one zone state is required");

        RuleForEach(x => x.ZoneStates).ChildRules(state =>
        {
            state.RuleFor(s => s.ZoneId).InclusiveBetween(1, 265);
            state.RuleFor(s => s.CurrentSupply).GreaterThanOrEqualTo(0);
            state.RuleFor(s => s.ActiveTrips).GreaterThanOrEqualTo(0);
        });

        RuleFor(x => x.TimeWindow)
    .GreaterThan(DateTime.UtcNow) // ده اللي بيسبب الخطأ لو التاريخ قديم
    .WithMessage("Time window must be in the future");
    }
}
