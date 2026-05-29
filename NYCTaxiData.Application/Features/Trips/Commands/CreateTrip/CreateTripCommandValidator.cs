using FluentValidation;

namespace NYCTaxiData.Application.Features.Trips.Commands.CreateTrip;

public sealed class CreateTripCommandValidator : AbstractValidator<CreateTripCommand>
{
    public CreateTripCommandValidator()
    {
        RuleFor(x => x.FareAmount)
            .GreaterThan(0)
            .WithMessage("FareAmount must be greater than 0.");

        RuleFor(x => x.TipAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TipAmount.HasValue)
            .WithMessage("TipAmount must be zero or positive.");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TotalAmount.HasValue)
            .WithMessage("TotalAmount must be zero or positive.");

        RuleFor(x => x.PickupLocationId)
            .GreaterThan(0)
            .When(x => x.PickupLocationId.HasValue)
            .WithMessage("PickupLocationId must be greater than 0.");

        RuleFor(x => x.DropoffLocationId)
            .GreaterThan(0)
            .When(x => x.DropoffLocationId.HasValue)
            .WithMessage("DropoffLocationId must be greater than 0.");

        RuleFor(x => x.DriverId)
            .NotEmpty()
            .When(x => x.DriverId.HasValue)
            .WithMessage("DriverId must be a valid GUID.");

        RuleFor(x => x)
            .Must(x => !x.PickupLocationId.HasValue || !x.DropoffLocationId.HasValue || x.PickupLocationId != x.DropoffLocationId)
            .WithMessage("Pickup and Dropoff locations must be different.");

        RuleFor(x => x)
            .Must(x => !x.StartedAt.HasValue || !x.EndedAt.HasValue || x.EndedAt >= x.StartedAt)
            .WithMessage("EndedAt must be greater than or equal to StartedAt.");
    }
}
