using FluentValidation;
using NYCTaxiData.Application.Features.Trips.Commands.EndTrip;

namespace NYCTaxiData.Application.Features.Trips.Commands.EndTrip
{
    public class EndTripCommandValidator : AbstractValidator<EndTripCommand>
    {
        public EndTripCommandValidator()
        {
            RuleFor(x => x.TripId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Trip ID must be greater than 0");

            RuleFor(x => x.BaseFare)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Base fare must be greater than 0");

            RuleFor(x => x.SurgeMultiplier)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Surge multiplier must be greater than 0");
        }
    }
}
