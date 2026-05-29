using FluentValidation;

namespace NYCTaxiData.Application.Features.Trips.Commands.CreateTrip
{
    public class CreateTripCommandValidator : AbstractValidator<CreateTripCommand>
    {
        public CreateTripCommandValidator()
        {
            RuleFor(x => x.DriverId)
                .NotEmpty().WithMessage("DriverId is required.");

            RuleFor(x => x.PickupLocationId)
                .GreaterThan(0).WithMessage("Valid PickupLocationId is required.");

            RuleFor(x => x.DropoffLocationId)
                .GreaterThan(0).WithMessage("Valid DropoffLocationId is required.");

            RuleFor(x => x.FareAmount)
                .GreaterThanOrEqualTo(0).WithMessage("FareAmount must be greater than or equal to 0.");

            RuleFor(x => x.TipAmount)
                .GreaterThanOrEqualTo(0).WithMessage("TipAmount must be greater than or equal to 0.");
        }
    }
}
