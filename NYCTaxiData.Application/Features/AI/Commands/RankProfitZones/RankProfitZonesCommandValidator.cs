using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Commands.RankProfitZones;

/// <summary>
/// Validator for <see cref="RankProfitZonesCommand"/>.
/// </summary>
public class RankProfitZonesCommandValidator : AbstractValidator<RankProfitZonesCommand>
{
    private static readonly string[] AllowedTreatmentTypes = new[]
    {
        "heavy_rain", "snow", "concert", "holiday", "road_closure", "sport_event", "strike"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="RankProfitZonesCommandValidator"/> class.
    /// </summary>
    public RankProfitZonesCommandValidator()
    {
        RuleFor(x => x.ZoneIds)
            .NotEmpty().WithMessage("At least one zone ID is required");

        RuleForEach(x => x.ZoneIds)
            .InclusiveBetween(1, 265).WithMessage("ZoneId must be between 1 and 265");

        RuleFor(x => x.CurrentHour)
            .InclusiveBetween(0, 23);

        RuleFor(x => x.DayOfWeek)
            .InclusiveBetween(0, 6);

        RuleFor(x => x.TopK)
            .Must((cmd, topK) => !topK.HasValue || topK.Value <= cmd.ZoneIds.Count)
            .WithMessage("TopK cannot exceed the number of zone IDs provided")
            .When(x => x.TopK.HasValue);
    }
}
