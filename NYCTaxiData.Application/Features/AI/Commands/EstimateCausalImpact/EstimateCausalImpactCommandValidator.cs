using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.EstimateCausalImpact;

/// <summary>
/// Validator for <see cref="EstimateCausalImpactCommand"/>.
/// </summary>
public class EstimateCausalImpactCommandValidator : AbstractValidator<EstimateCausalImpactCommand>
{
    private static readonly string[] AllowedTreatmentTypes = new[]
    {
        "heavy_rain", "snow", "concert", "holiday", "road_closure", "sport_event", "strike"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="EstimateCausalImpactCommandValidator"/> class.
    /// </summary>
    public EstimateCausalImpactCommandValidator()
    {
        RuleFor(x => x.ZoneId)
            .InclusiveBetween(1, 265);

        RuleFor(x => x.EventDate)
            .NotEmpty();

        RuleFor(x => x.TreatmentType)
            .NotEmpty()
            .MaximumLength(50)
            .Must(t => AllowedTreatmentTypes.Contains(t))
            .WithMessage($"TreatmentType must be one of: {string.Join(", ", AllowedTreatmentTypes)}");

        RuleFor(x => x.BaselineDemand)
            .GreaterThanOrEqualTo(0);
    }
}
