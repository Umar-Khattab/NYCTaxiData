using FluentValidation;
using NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch;

namespace NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch
{
    public class ManualDispatchCommandValidator : AbstractValidator<ManualDispatchCommand>
    {
        public ManualDispatchCommandValidator()
        {
            RuleFor(x => x.DriverId)
                .NotEmpty()
                .WithMessage("Driver ID is required");

            RuleFor(x => x.PickupZoneId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Pickup Zone ID must be greater than 0");

            RuleFor(x => x.DropoffZoneId)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Dropoff Zone ID must be greater than 0");

            RuleFor(x => x)
                .Must(x => x.PickupZoneId != x.DropoffZoneId)
                .WithMessage("Pickup and Dropoff zones must be different");

            RuleFor(x => x.PassengerName)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(100)
                .WithMessage("Passenger name must be between 2 and 100 characters");

            RuleFor(x => x.PassengerPhone)
                .NotEmpty()
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Passenger phone must be a valid phone number (E.164 format)");
        }
    }
}
