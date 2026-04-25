using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Analytics.Commands.UpdateSystemThresholds
{
    public sealed record UpdateSystemThresholdsCommand(
        UpdateSurgeMultipliersDto SurgeMultipliers,
        UpdateDispatchRadiusThresholdsDto DispatchRadiusKm)
        : IRequest<Result<UpdateSystemThresholdsResultDto>>, ITransactionalCommand;

    public sealed record UpdateSurgeMultipliersDto(
        decimal Normal,
        decimal Elevated,
        decimal Critical);

    public sealed record UpdateDispatchRadiusThresholdsDto(
        decimal Normal,
        decimal Elevated,
        decimal Critical);

    public sealed record UpdateSystemThresholdsResultDto(
        UpdateSurgeMultipliersDto SurgeMultipliers,
        UpdateDispatchRadiusThresholdsDto DispatchRadiusKm,
        DateTime UpdatedAtUtc);
}
