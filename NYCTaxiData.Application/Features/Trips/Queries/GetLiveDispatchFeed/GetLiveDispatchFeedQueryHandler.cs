using MediatR;
using AutoMapper;
using System.Linq.Expressions;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed
{
    public class GetLiveDispatchFeedQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetLiveDispatchFeedQuery, LiveDispatchFeedResultDto>
    {
        public async Task<LiveDispatchFeedResultDto> Handle(
            GetLiveDispatchFeedQuery request,
            CancellationToken cancellationToken)
        {
            // Calculate the time window for recent trips
            var cutoffTime = DateTime.UtcNow.AddMinutes(-request.MinutesWindow);

            // Get recent trips within the time window, ordered by most recent first
            var recentTrips = await _unitOfWork.Trips.FindByConditionWithIncludesAsync(
                predicate: t => t.StartedAt >= cutoffTime,
                includes: new Expression<Func<Trip, object>>[]
                {
                    t => t.Driver!,
                    t => t.PickupLocation!,
                    t => t.DropoffLocation!
                });

            // Apply limit and convert to DTOs using AutoMapper
            var dispatchItems = recentTrips
                .OrderByDescending(t => t.StartedAt)
                .Take(request.Limit)
                .Select(trip => _mapper.Map<DispatchFeedItemDto>(trip))
                .ToList();

            return new LiveDispatchFeedResultDto
            {
                Items = dispatchItems,
                TotalCount = dispatchItems.Count,
                RetrievedAt = DateTime.UtcNow
            };
        }
    }
}