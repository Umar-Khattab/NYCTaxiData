using FluentValidation;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetActiveFleet;

public sealed class GetActiveFleetQueryValidator : AbstractValidator<GetActiveFleetQuery>
{
    public GetActiveFleetQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(200)
            .WithMessage("PageSize must be between 1 and 200.");
    }
}
