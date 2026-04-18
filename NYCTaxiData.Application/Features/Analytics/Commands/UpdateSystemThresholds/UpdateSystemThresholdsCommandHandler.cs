using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Features.Analytics.Common;
using NYCTaxiData.Application.Features.Analytics.Queries.GetSystemThresholds;
using System.Text.Json;

namespace NYCTaxiData.Application.Features.Analytics.Commands.UpdateSystemThresholds
{
    public sealed class UpdateSystemThresholdsCommandHandler(IDistributedCache cache)
        : IRequestHandler<UpdateSystemThresholdsCommand, Result<UpdateSystemThresholdsResultDto>>
    {
        private readonly IDistributedCache _cache = cache;

        public async Task<Result<UpdateSystemThresholdsResultDto>> Handle(UpdateSystemThresholdsCommand request, CancellationToken cancellationToken)
        {
            var updatedAtUtc = DateTime.UtcNow;

            var cachedThresholds = new SystemThresholdsDto(
                SurgeMultipliers: new SurgeMultipliersDto(
                    Normal: request.SurgeMultipliers.Normal,
                    Elevated: request.SurgeMultipliers.Elevated,
                    Critical: request.SurgeMultipliers.Critical),
                DispatchRadiusKm: new DispatchRadiusThresholdsDto(
                    Normal: request.DispatchRadiusKm.Normal,
                    Elevated: request.DispatchRadiusKm.Elevated,
                    Critical: request.DispatchRadiusKm.Critical),
                LastUpdatedUtc: updatedAtUtc);

            var json = JsonSerializer.Serialize(cachedThresholds);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
            };

            await _cache.SetStringAsync(AnalyticsCacheKeys.SystemThresholds, json, options, cancellationToken);

            var response = new UpdateSystemThresholdsResultDto(
                SurgeMultipliers: request.SurgeMultipliers,
                DispatchRadiusKm: request.DispatchRadiusKm,
                UpdatedAtUtc: updatedAtUtc);

            return Result<UpdateSystemThresholdsResultDto>.Success(response);
        }
    }
}
