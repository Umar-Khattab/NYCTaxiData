using FluentValidation;

namespace NYCTaxiData.Application.Features.Trips.Commands.UpdateTrip
{
    public class UpdateTripCommandValidator : AbstractValidator<UpdateTripCommand>
    {
        public UpdateTripCommandValidator()
        {
            RuleFor(x => x.TripId)
                .GreaterThan(0).WithMessage("Valid TripId is required.");

            RuleFor(x => x.FareAmount)
                .GreaterThanOrEqualTo(0).WithMessage("FareAmount must be greater than or equal to 0.");

            RuleFor(x => x.TipAmount)
                .GreaterThanOrEqualTo(0).WithMessage("TipAmount must be greater than or equal to 0.");

            RuleFor(x => x.ProcessStatus)
                .NotEmpty().WithMessage("ProcessStatus is required.");
        }
    }
}
