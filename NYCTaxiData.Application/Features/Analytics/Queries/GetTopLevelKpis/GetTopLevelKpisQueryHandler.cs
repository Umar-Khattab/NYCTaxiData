using MediatR;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;

namespace NYCTaxiData.Application.Features.Analytics.Queries.GetTopLevelKpis
{
    public sealed class GetTopLevelKpisQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetTopLevelKpisQuery, Result<TopLevelKpisDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<TopLevelKpisDto>> Handle(GetTopLevelKpisQuery request, CancellationToken cancellationToken)
        {
            var utcToday = DateTime.UtcNow.Date;
            var utcTomorrow = utcToday.AddDays(1);

            var activeDriversCount = await _unitOfWork.Drivers.CountAsync(driver =>
                driver.Status == CurrentStatus.Available || driver.Status == CurrentStatus.On_Trip);

            var todaysTrips = await _unitOfWork.Trips.FindByConditionAsync(trip =>
                trip.EndedAt.HasValue
                && trip.EndedAt.Value >= utcToday
                && trip.EndedAt.Value < utcTomorrow);

            var totalDailyRevenue = todaysTrips.Sum(trip => trip.ActualFare ?? 0m);

            var todaySimulationResults = await _unitOfWork.SimulationResults.FindByConditionAsync(result =>
                result.ComputedAt.HasValue
                && result.ComputedAt.Value >= utcToday
                && result.ComputedAt.Value < utcTomorrow
                && result.EtaP50Sec.HasValue);

            var averageQueueTimeSeconds = todaySimulationResults
                .Select(result => (double)(result.EtaP50Sec ?? 0m))
                .DefaultIfEmpty(0d)
                .Average();

            var kpis = new TopLevelKpisDto(
                ActiveDriversCount: activeDriversCount,
                TotalDailyRevenue: totalDailyRevenue,
                AverageQueueTimeMinutes: Math.Round((decimal)(averageQueueTimeSeconds / 60d), 2));

            return Result<TopLevelKpisDto>.Success(kpis);
        }
    }
}
