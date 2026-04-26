using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictRevenue;

/// <summary>
/// Validator for <see cref="PredictRevenueCommand"/>.
/// </summary>
public class PredictRevenueCommandValidator : AbstractValidator<PredictRevenueCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PredictRevenueCommandValidator"/> class.
    /// </summary>
    public PredictRevenueCommandValidator()
    {
        RuleFor(x => x.Zones)
            .NotEmpty().WithMessage("At least one zone is required")
            .Must(z => z.Count <= 265).WithMessage("Maximum 265 zones allowed");

        RuleForEach(x => x.Zones).ChildRules(zone =>
        {
            zone.RuleFor(z => z.ZoneId).InclusiveBetween(1, 265);
            zone.RuleFor(z => z.AvgFare).GreaterThanOrEqualTo(0);
            zone.RuleFor(z => z.TipRate).InclusiveBetween(0, 1);
            zone.RuleFor(z => z.ForecastedDemand6h).GreaterThanOrEqualTo(0);
        });
    }
}
