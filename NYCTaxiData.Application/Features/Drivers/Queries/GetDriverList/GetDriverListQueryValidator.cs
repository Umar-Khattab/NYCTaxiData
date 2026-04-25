using FluentValidation;
using NYCTaxiData.Domain.Enums;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;

public sealed class GetDriverListQueryValidator : AbstractValidator<GetDriverListQuery>
{
    public GetDriverListQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(200)
            .WithMessage("PageSize must be between 1 and 200.");

        RuleFor(x => x.ZoneId)
            .GreaterThan(0)
            .When(x => x.ZoneId.HasValue)
            .WithMessage("ZoneId must be greater than 0.");

        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be one of: Available, On_Trip, Offline.");
    }

    private static bool BeValidStatus(string? status)
    {
        return Enum.TryParse<CurrentStatus>(status, true, out _);
    }
}
