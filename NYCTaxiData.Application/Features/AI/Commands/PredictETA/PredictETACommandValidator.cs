using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictETA;

/// <summary>
/// Validator for <see cref="PredictETACommand"/>.
/// </summary>
public class PredictETACommandValidator : AbstractValidator<PredictETACommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PredictETACommandValidator"/> class.
    /// </summary>
    public PredictETACommandValidator()
    {
        RuleFor(x => x.Routes)
            .NotEmpty().WithMessage("At least one route is required");

        RuleForEach(x => x.Routes).ChildRules(route =>
        {
            route.RuleFor(r => r.PickupZoneId).InclusiveBetween(1, 265);
            route.RuleFor(r => r.DropoffZoneId).InclusiveBetween(1, 265);
        });
    }
}
