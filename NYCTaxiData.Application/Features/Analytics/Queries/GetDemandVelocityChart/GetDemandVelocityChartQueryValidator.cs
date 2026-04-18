using FluentValidation;

namespace NYCTaxiData.Application.Features.Analytics.Queries.GetDemandVelocityChart
{
    public sealed class GetDemandVelocityChartQueryValidator : AbstractValidator<GetDemandVelocityChartQuery>
    {
        public GetDemandVelocityChartQueryValidator()
        {
            RuleFor(query => query.Hours)
                .InclusiveBetween(1, 168)
                .WithMessage("Hours must be between 1 and 168.");

            RuleFor(query => query.ZoneId)
                .GreaterThan(0)
                .When(query => query.ZoneId.HasValue)
                .WithMessage("ZoneId must be greater than 0 when provided.");
        }
    }
}
