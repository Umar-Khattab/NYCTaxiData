using FluentValidation;

namespace NYCTaxiData.Application.Features.AI.Queries.GetSimulationResult;

/// <summary>
/// Validator for <see cref="GetSimulationResultQuery"/>.
/// </summary>
public class GetSimulationResultQueryValidator : AbstractValidator<GetSimulationResultQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSimulationResultQueryValidator"/> class.
    /// </summary>
    public GetSimulationResultQueryValidator()
    {
        RuleFor(x => x.SimulationId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
