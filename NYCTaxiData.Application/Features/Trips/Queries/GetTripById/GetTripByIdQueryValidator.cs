using FluentValidation;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetTripById;

public sealed class GetTripByIdQueryValidator : AbstractValidator<GetTripByIdQuery>
{
    public GetTripByIdQueryValidator()
    {
        RuleFor(x => x.TripId)
            .GreaterThan(0)
            .WithMessage("TripId must be greater than 0.");
    }
}
