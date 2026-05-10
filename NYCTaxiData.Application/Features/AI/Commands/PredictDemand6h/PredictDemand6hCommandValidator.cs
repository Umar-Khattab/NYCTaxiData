using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictDemand6h;

/// <summary>
/// Validator for <see cref="PredictDemand6hCommand"/>.
/// </summary>
public class PredictDemand6hCommandValidator : AbstractValidator<PredictDemand6hCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PredictDemand6hCommandValidator"/> class.
    /// </summary>
    public PredictDemand6hCommandValidator()
    {
        RuleFor(x => x.Zones)
            .NotEmpty().WithMessage("At least one zone is required")
            .Must(z => z.Count <= 265).WithMessage("Maximum 265 zones allowed");

        RuleForEach(x => x.Zones).ChildRules(zone =>
        {
            zone.RuleFor(z => z.ZoneId).InclusiveBetween(1, 265);
            zone.RuleFor(z => z.PickupHour).InclusiveBetween(0, 23);
            zone.RuleFor(z => z.DayOfWeek).InclusiveBetween(0, 6);
            zone.RuleFor(z => z.RainMm).GreaterThanOrEqualTo(0);
        });
    }
}
