using FluentValidation;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;

public sealed class GetDriverProfileQueryValidator : AbstractValidator<GetDriverProfileQuery>
{
    public GetDriverProfileQueryValidator()
    {
        RuleFor(x => x.DriverId)
            .NotEmpty()
            .WithMessage("DriverId is required.");
    }
}
