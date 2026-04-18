using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Analytics.Queries.GetSystemThresholds
{
    public sealed record GetSystemThresholdsQuery : IRequest<Result<SystemThresholdsDto>>, ICacheableQuery;

    public sealed record SystemThresholdsDto(
        SurgeMultipliersDto SurgeMultipliers,
        DispatchRadiusThresholdsDto DispatchRadiusKm,
        DateTime LastUpdatedUtc);

    public sealed record SurgeMultipliersDto(
        decimal Normal,
        decimal Elevated,
        decimal Critical);

    public sealed record DispatchRadiusThresholdsDto(
        decimal Normal,
        decimal Elevated,
        decimal Critical);
}
