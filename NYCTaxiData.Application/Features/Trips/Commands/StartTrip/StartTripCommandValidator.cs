using FluentValidation;
using NYCTaxiData.Application.Features.Trips.Commands.StartTrip;

namespace NYCTaxiData.Application.Features.Trips.Commands.StartTrip
{
    public class StartTripCommandValidator : AbstractValidator<StartTripCommand>
    {
        public StartTripCommandValidator()
        {
            RuleFor(x => x.DriverId)
                .NotEmpty()
                .WithMessage("Driver ID is required");

            RuleFor(x => x.PickupLocationId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Pickup Location ID must be greater than 0");

            RuleFor(x => x.DropoffLocationId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Dropoff Location ID must be greater than 0");

            RuleFor(x => x)
                .Must(x => x.PickupLocationId != x.DropoffLocationId)
                .WithMessage("Pickup and Dropoff locations must be different");
        }
    }
}
