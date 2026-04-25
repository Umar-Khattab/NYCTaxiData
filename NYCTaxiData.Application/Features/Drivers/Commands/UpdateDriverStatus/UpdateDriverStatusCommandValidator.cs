using FluentValidation;
using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Application.Features.Drivers.Commands.UpdateDriverStatus
{
    public sealed class UpdateDriverStatusCommandValidator : AbstractValidator<UpdateDriverStatusCommand>
    {
        public UpdateDriverStatusCommandValidator()
        {
            RuleFor(x => x.DriverId)
                .NotEmpty()
                .WithMessage("DriverId is required.");

            RuleFor(x => x.Status)
                .NotEmpty()
                .Must(BeValidStatus)
                .WithMessage("Status must be one of: Available, On_Trip, Offline.");

            RuleFor(x => x.CurrentLat)
                .InclusiveBetween(-90, 90)
                .WithMessage("CurrentLat must be between -90 and 90.");

            RuleFor(x => x.CurrentLng)
                .InclusiveBetween(-180, 180)
                .WithMessage("CurrentLng must be between -180 and 180.");
        }

        private static bool BeValidStatus(string status)
            => Enum.TryParse<CurrentStatus>(status, true, out _);
    }
}
