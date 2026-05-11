using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Domain.Specifications.Trips;

namespace NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed
{
    public class GetLiveDispatchFeedQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetLiveDispatchFeedQuery, Result<LiveDispatchFeedResultDto>>
    {
        public async Task<Result<LiveDispatchFeedResultDto>> Handle(
            GetLiveDispatchFeedQuery request,
            CancellationToken cancellationToken)
        {
            // 1. استدعاء السائقين مع الـ Includes (User & Trips) في ضربة واحدة
            var spec = new DispatchFeedSpec(request.Limit);
            var activeDrivers = await _unitOfWork.Drivers.GetAllBySpecAsync(spec);

            // 2. تحويل البيانات لـ DTO
            var dispatchItems = _mapper.Map<List<DispatchFeedItemDto>>(activeDrivers);

            // 3. تحديث حالة الرحلة لكل سائق من الـ Memory (بدون Queries إضافية)
            foreach (var item in dispatchItems)
            {
                // البحث عن السائق في اللستة اللي رجعت
                var driver = activeDrivers.FirstOrDefault(d => d.User?.PhoneNumber == item.PhoneNumber);

                if (driver != null && driver.Trips != null)
                {
                    // الحصول على أحدث رحلة من الـ Collection اللي حصل لها Include فعلياً
                    var lastTrip = driver.Trips
                        .OrderByDescending(t => t.StartedAt)
                        .FirstOrDefault();

                    if (lastTrip != null)
                    {
                        item.CurrentTripId = lastTrip.TripId;
                        // تحديد الحالة بناءً على وجود تاريخ نهاية للرحلة
                        item.Status = lastTrip.EndedAt == null ? "On Trip" : "Available";
                        item.LastUpdated = lastTrip.StartedAt;
                    }
                    else
                    {
                        item.Status = "Available";
                    }
                }
            }

            // 4. تجهيز النتيجة النهائية
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