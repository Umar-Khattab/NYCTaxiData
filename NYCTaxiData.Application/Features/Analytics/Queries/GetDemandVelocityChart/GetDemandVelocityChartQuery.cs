using MediatR;
using NYCTaxiData.Application.Common;

namespace NYCTaxiData.Application.Features.Analytics.Queries.GetDemandVelocityChart
{
    public sealed record GetDemandVelocityChartQuery(
        int? ZoneId = null,
        int Hours = 24) : IRequest<Result<DemandVelocityChartDto>>;

    public sealed record DemandVelocityChartDto(
        int? ZoneId,
        DateTime FromUtc,
        DateTime ToUtc,
        IReadOnlyCollection<DemandVelocityPointDto> Points);

    public sealed record DemandVelocityPointDto(
        DateTime TimeBucketUtc,
        double PredictedPickupCount,
        double P50,
        double P90);
}
