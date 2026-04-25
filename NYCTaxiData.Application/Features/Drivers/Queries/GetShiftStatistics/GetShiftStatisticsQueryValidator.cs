using FluentValidation;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetShiftStatistics;

public sealed class GetShiftStatisticsQueryValidator : AbstractValidator<GetShiftStatisticsQuery>
{
    public GetShiftStatisticsQueryValidator()
    {
        RuleFor(x => x.DriverId)
            .NotEmpty()
            .WithMessage("DriverId is required.");

        RuleFor(x => x)
            .Must(x => !x.ShiftStartUtc.HasValue || !x.ShiftEndUtc.HasValue || x.ShiftEndUtc.Value > x.ShiftStartUtc.Value)
            .WithMessage("ShiftEndUtc must be greater than ShiftStartUtc when both are provided.");
    }
}
