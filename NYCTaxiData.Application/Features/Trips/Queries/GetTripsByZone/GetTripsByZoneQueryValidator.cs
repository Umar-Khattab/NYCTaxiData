using FluentValidation;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripsByZone;

public sealed class GetTripsByZoneQueryValidator : AbstractValidator<GetTripsByZoneQuery>
{
    public GetTripsByZoneQueryValidator()
    {
        RuleFor(x => x.ZoneId)
            .GreaterThan(0)
            .WithMessage("ZoneId must be greater than 0.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(200)
            .WithMessage("PageSize must be between 1 and 200.");
    }
}
