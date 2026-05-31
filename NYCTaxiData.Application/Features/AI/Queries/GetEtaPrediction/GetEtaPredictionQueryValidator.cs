using FluentValidation;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Application.Features.AI.Queries.GetEtaPrediction;

public class GetEtaPredictionQueryValidator : AbstractValidator<GetEtaPredictionQuery>
{
    public GetEtaPredictionQueryValidator()
    {
        RuleFor(x => x.Routes)
            .NotNull().WithMessage("Routes is required.")
            .NotEmpty().WithMessage("At least one route is required.")
            .Must(r => r.Count <= 50).WithMessage("A maximum of 50 routes can be submitted per request.");

        RuleForEach(x => x.Routes).ChildRules(route =>
        {
            route.RuleFor(r => r.PickupZoneId)
                .GreaterThan(0).WithMessage("PickupZoneId must be a positive integer.");

            route.RuleFor(r => r.DropoffZoneId)
                .GreaterThan(0).WithMessage("DropoffZoneId must be a positive integer.")
                .NotEqual(r => r.PickupZoneId).WithMessage("DropoffZoneId must differ from PickupZoneId.");

            route.RuleFor(r => r.TargetTime)
                .NotEmpty().WithMessage("TargetTime is required.")
                .GreaterThan(new DateTime(2000, 1, 1)).WithMessage("TargetTime must be a valid date after 2000-01-01.");
        });
    }
}
