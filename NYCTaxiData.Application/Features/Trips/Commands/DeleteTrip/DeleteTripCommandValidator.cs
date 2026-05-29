using FluentValidation;

namespace NYCTaxiData.Application.Features.Trips.Commands.DeleteTrip;

public sealed class DeleteTripCommandValidator : AbstractValidator<DeleteTripCommand>
{
    public DeleteTripCommandValidator()
    {
        RuleFor(x => x.TripId)
            .GreaterThan(0)
            .WithMessage("TripId must be greater than 0.");
    }
}
