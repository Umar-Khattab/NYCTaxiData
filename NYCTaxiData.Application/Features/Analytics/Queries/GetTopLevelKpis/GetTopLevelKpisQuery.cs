using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;

namespace NYCTaxiData.Application.Features.Analytics.Queries.GetTopLevelKpis
{
    public sealed record GetTopLevelKpisQuery : IRequest<Result<TopLevelKpisDto>>, ICacheableQuery;

    public sealed record TopLevelKpisDto(
        int ActiveDriversCount,
        decimal TotalDailyRevenue,
        decimal AverageQueueTimeMinutes);
}
