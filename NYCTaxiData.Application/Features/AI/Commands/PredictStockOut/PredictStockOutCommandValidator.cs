using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictStockOut;

/// <summary>
/// Validator for <see cref="PredictStockOutCommand"/>.
/// </summary>
public class PredictStockOutCommandValidator : AbstractValidator<PredictStockOutCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PredictStockOutCommandValidator"/> class.
    /// </summary>
    public PredictStockOutCommandValidator()
    {
        RuleFor(x => x.Zones)
            .NotEmpty().WithMessage("At least one zone is required")
            .Must(z => z.Count <= 265).WithMessage("Maximum 265 zones allowed");

        RuleForEach(x => x.Zones).ChildRules(zone =>
        {
            zone.RuleFor(z => z.ZoneId).InclusiveBetween(1, 265);
            zone.RuleFor(z => z.Hour).InclusiveBetween(0, 23);
            zone.RuleFor(z => z.DayOfWeek).InclusiveBetween(0, 6);
            zone.RuleFor(z => z.PickupCount).NotNull();
            zone.RuleFor(z => z.DropoffCount).NotNull();
            zone.RuleFor(z => z.NetFlow).NotNull();
            zone.RuleFor(z => z.ActivityRatio).NotNull();
            zone.RuleFor(z => z.RainMm).GreaterThanOrEqualTo(0);
        });
    }
}
