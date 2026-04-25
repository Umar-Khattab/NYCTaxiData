using FluentValidation;

namespace NYCTaxiData.Application.Features.Drivers.Commands.SyncOfflineData;

public sealed class SyncOfflineDataCommandValidator : AbstractValidator<SyncOfflineDataCommand>
{
    public SyncOfflineDataCommandValidator()
    {
        RuleFor(x => x.DriverId)
            .NotEmpty()
            .WithMessage("DriverId is required.");

        RuleFor(x => x.Trips)
            .NotNull()
            .WithMessage("Trips payload is required.");

        RuleForEach(x => x.Trips)
            .SetValidator(new OfflineTripDtoValidator());
    }
}

public sealed class OfflineTripDtoValidator : AbstractValidator<OfflineTripDto>
{
    public OfflineTripDtoValidator()
    {
        RuleFor(x => x.LocalTripId)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("LocalTripId is required and must not exceed 100 characters.");

        RuleFor(x => x.PickupLocationId)
            .GreaterThan(0)
            .WithMessage("PickupLocationId must be greater than 0.");

        RuleFor(x => x.DropoffLocationId)
            .GreaterThan(0)
            .WithMessage("DropoffLocationId must be greater than 0.");

        RuleFor(x => x)
            .Must(x => x.PickupLocationId != x.DropoffLocationId)
            .WithMessage("PickupLocationId and DropoffLocationId must be different.");

        RuleFor(x => x.StartedAt)
            .NotEqual(default(DateTime))
            .WithMessage("StartedAt is required.");

        RuleFor(x => x.EndedAt)
            .NotEqual(default(DateTime))
            .WithMessage("EndedAt is required.")
            .GreaterThan(x => x.StartedAt)
            .WithMessage("EndedAt must be greater than StartedAt.");

        RuleFor(x => x.ActualFare)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ActualFare must be greater than or equal to 0.");
    }
}
