using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Analytics.Queries.GetDemandVelocityChart
{
    public sealed class GetDemandVelocityChartQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetDemandVelocityChartQuery, Result<DemandVelocityChartDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<DemandVelocityChartDto>> Handle(GetDemandVelocityChartQuery request, CancellationToken cancellationToken)
        {
            var toUtc = DateTime.UtcNow;
            var fromUtc = toUtc.AddHours(-request.Hours);

            var points = await _unitOfWork.DemandPredictions.FindByConditionAsync(prediction =>
                prediction.TimeBucket6h >= fromUtc
                && prediction.TimeBucket6h <= toUtc
                && (!request.ZoneId.HasValue || prediction.ZoneId == request.ZoneId.Value));

            var chartPoints = points
                .GroupBy(prediction => prediction.TimeBucket6h)
                .OrderBy(group => group.Key)
                .Select(group => new DemandVelocityPointDto(
                    TimeBucketUtc: group.Key,
                    PredictedPickupCount: Math.Round(group.Sum(item => item.PredictedPickupCount ?? 0d), 2),
                    P50: Math.Round(group.Average(item => item.P50 ?? 0d), 2),
                    P90: Math.Round(group.Average(item => item.P90 ?? 0d), 2)))
                .ToList();

            var response = new DemandVelocityChartDto(
                ZoneId: request.ZoneId,
                FromUtc: fromUtc,
                ToUtc: toUtc,
                Points: chartPoints);

            return Result<DemandVelocityChartDto>.Success(response);
        }
    }
}
