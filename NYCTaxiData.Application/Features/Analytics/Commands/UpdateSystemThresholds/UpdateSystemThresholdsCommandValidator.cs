using FluentValidation;

namespace NYCTaxiData.Application.Features.Analytics.Commands.UpdateSystemThresholds
{
    public sealed class UpdateSystemThresholdsCommandValidator : AbstractValidator<UpdateSystemThresholdsCommand>
    {
        public UpdateSystemThresholdsCommandValidator()
        {
            RuleFor(command => command.SurgeMultipliers)
                .NotNull();

            RuleFor(command => command.DispatchRadiusKm)
                .NotNull();

            When(command => command.SurgeMultipliers is not null, () =>
            {
                RuleFor(command => command.SurgeMultipliers.Normal)
                    .InclusiveBetween(1.00m, 2.00m);

                RuleFor(command => command.SurgeMultipliers.Elevated)
                    .GreaterThan(command => command.SurgeMultipliers.Normal)
                    .LessThanOrEqualTo(3.00m);

                RuleFor(command => command.SurgeMultipliers.Critical)
                    .GreaterThan(command => command.SurgeMultipliers.Elevated)
                    .LessThanOrEqualTo(5.00m);
            });

            When(command => command.DispatchRadiusKm is not null, () =>
            {
                RuleFor(command => command.DispatchRadiusKm.Normal)
                    .GreaterThan(0m)
                    .LessThanOrEqualTo(10m);

                RuleFor(command => command.DispatchRadiusKm.Elevated)
                    .GreaterThan(command => command.DispatchRadiusKm.Normal)
                    .LessThanOrEqualTo(20m);

                RuleFor(command => command.DispatchRadiusKm.Critical)
                    .GreaterThan(command => command.DispatchRadiusKm.Elevated)
                    .LessThanOrEqualTo(30m);
            });
        }
    }
}
