using FluentValidation;

namespace NYCTaxiData.Application.Features.Trips.Commands.StartTrip
{
    public class StartTripCommandValidator : AbstractValidator<StartTripCommand>
    {
        public StartTripCommandValidator()
        { 
            RuleFor(x => x.TripId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Valid Trip ID is required to start the trip");
             
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
                .WithMessage("Pickup and Dropoff locations must be different") 
                .When(x => x.PickupLocationId > 0 && x.DropoffLocationId > 0);
        }
    }
}