using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsTrip;
using NYCTaxiData.Infrastructure.Services.Specifications.Trips;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed
{
    public class GetLiveDispatchFeedQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetLiveDispatchFeedQuery, Result<LiveDispatchFeedResultDto>>
    {
        public async Task<Result<LiveDispatchFeedResultDto>> Handle(
            GetLiveDispatchFeedQuery request,
            CancellationToken cancellationToken)
        {
            var spec = new DispatchFeedSpec(request.Limit);
            var activeDrivers = await _unitOfWork.Drivers.GetAllBySpecAsync(spec);

            var dispatchItems = _mapper.Map<List<DispatchFeedItemDto>>(activeDrivers);

            foreach (var item in dispatchItems)
            { 
                var driver = activeDrivers.FirstOrDefault(d => d.User?.PhoneNumber == item.PhoneNumber);

                if (driver != null)
                {
                    var driverId = driver.UserId;
                    var activeTrip = await _unitOfWork.Trips.GetBySpecAsync(new TripsByDriverSpec(driverId));

                    if (activeTrip != null)
                    {
                        item.CurrentTripId = activeTrip.TripId;
                        item.Status = activeTrip.EndedAt == null ? "On Trip" : "Available";
                        item.LastUpdated = activeTrip.StartedAt;
                    }
                }
            }

            var result = new LiveDispatchFeedResultDto
            {
                Items = dispatchItems,
                TotalCount = dispatchItems.Count,
                RetrievedAt = DateTime.UtcNow
            };

            return Result<LiveDispatchFeedResultDto>.Success(result, "Live dispatch feed retrieved successfully");
        }
    }
}