using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Interfaces;
using System.Text.Json;

namespace NYCTaxiData.Application.Features.Analytics.Queries.GetSystemThresholds
{
    public sealed class GetSystemThresholdsQueryHandler(IUnitOfWork unitOfWork, IDistributedCache cache)
        : IRequestHandler<GetSystemThresholdsQuery, Result<SystemThresholdsDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDistributedCache _cache = cache;

        public async Task<Result<SystemThresholdsDto>> Handle(GetSystemThresholdsQuery request, CancellationToken cancellationToken)
        {
            var cachedJson = await _cache.GetStringAsync(AnalyticsCacheKeys.SystemThresholds, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cachedJson))
            {
                var cachedThresholds = JsonSerializer.Deserialize<SystemThresholdsDto>(cachedJson);
                if (cachedThresholds is not null)
                {
                    return Result<SystemThresholdsDto>.Success(cachedThresholds);
                }
            }

            var recentPredictions = await _unitOfWork.DemandPredictions.GetPagedAsync(
                pageNumber: 1,
                pageSize: 100,
                orderBy: query => query.OrderByDescending(item => item.TimeBucket6h));

            var hasDemandSignal = recentPredictions.Items.Any(item => item.P50.HasValue && item.P90.HasValue && item.P50 > 0d);

            var criticalMultiplier = hasDemandSignal
                ? Math.Round((decimal)recentPredictions.Items
                    .Where(item => item.P50.HasValue && item.P90.HasValue && item.P50 > 0d)
                    .Select(item => item.P90!.Value / item.P50!.Value)
                    .DefaultIfEmpty(2.40d)
                    .Average(), 2)
                : 2.40m;

            criticalMultiplier = Math.Clamp(criticalMultiplier, 2.00m, 3.50m);

            var systemThresholds = new SystemThresholdsDto(
                SurgeMultipliers: new SurgeMultipliersDto(
                    Normal: 1.00m,
                    Elevated: 1.50m,
                    Critical: criticalMultiplier),
                DispatchRadiusKm: new DispatchRadiusThresholdsDto(
                    Normal: 2.0m,
                    Elevated: 4.0m,
                    Critical: 7.0m),
                LastUpdatedUtc: DateTime.UtcNow);

            return Result<SystemThresholdsDto>.Success(systemThresholds);
        }
    }
}
