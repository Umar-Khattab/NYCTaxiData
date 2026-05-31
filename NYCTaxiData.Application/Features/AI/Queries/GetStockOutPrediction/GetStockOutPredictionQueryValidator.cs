using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Queries.GetStockOutPrediction;

public class GetStockOutPredictionQueryValidator : AbstractValidator<GetStockOutPredictionQuery>
{
    public GetStockOutPredictionQueryValidator()
    {
        RuleFor(x => x.ZoneIds)
            .NotNull().WithMessage("ZoneIds is required.")
            .NotEmpty().WithMessage("At least one zone ID is required.")
            .Must(ids => ids.Count <= 100).WithMessage("A maximum of 100 zone IDs can be submitted per request.")
            .Must(ids => ids.All(id => id > 0)).WithMessage("All zone IDs must be positive integers.");

        RuleFor(x => x.TargetTime)
            .NotEmpty().WithMessage("TargetTime is required.")
            .GreaterThan(new DateTime(2000, 1, 1)).WithMessage("TargetTime must be a valid date after 2000-01-01.");
    }
}
