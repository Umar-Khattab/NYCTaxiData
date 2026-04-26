using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictDemand15Min;

/// <summary>
/// Validator for <see cref="PredictDemand15MinCommand"/>.
/// </summary>
public class PredictDemand15MinCommandValidator : AbstractValidator<PredictDemand15MinCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PredictDemand15MinCommandValidator"/> class.
    /// </summary>
    public PredictDemand15MinCommandValidator()
    {
        RuleFor(x => x.Zones)
            .NotEmpty().WithMessage("At least one zone is required")
            .Must(z => z.Count <= 265).WithMessage("Maximum 265 zones allowed");

        RuleForEach(x => x.Zones).ChildRules(zone =>
        {
            zone.RuleFor(z => z.ZoneId).InclusiveBetween(1, 265);
            zone.RuleFor(z => z.Hour).InclusiveBetween(0, 23);
            zone.RuleFor(z => z.Minute).InclusiveBetween(0, 59);
            zone.RuleFor(z => z.DayOfWeek).InclusiveBetween(0, 6);
            zone.RuleFor(z => z.Month).InclusiveBetween(1, 12);
            zone.RuleFor(z => z.RainMm).GreaterThanOrEqualTo(0);
        });
    }
}
