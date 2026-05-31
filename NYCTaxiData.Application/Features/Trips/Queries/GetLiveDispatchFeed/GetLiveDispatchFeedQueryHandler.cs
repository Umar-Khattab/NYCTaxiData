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
            try
            {
                // 1. جلب السائقين من الداتابيز مع الـ Includes المحددة في الـ Spec
                var spec = new DispatchFeedSpec(request.Limit);
                var activeDrivers = await _unitOfWork.Drivers.GetAllBySpecAsync(spec);

                if (activeDrivers == null)
                    return Result<LiveDispatchFeedResultDto>.Success(new LiveDispatchFeedResultDto { Items = new List<DispatchFeedItemDto>() }, "No active drivers found");

                var dispatchItems = new List<DispatchFeedItemDto>();

                // 2. بناء الـ DTO خطوة بخطوة بالاعتماد على الـ UserId المضمون
                foreach (var driver in activeDrivers)
                {
                    var lastTrip = driver.Trips?
                        .Where(t => t.StartedAt != null)
                        .OrderByDescending(t => t.StartedAt)
                        .FirstOrDefault();

                    var item = new DispatchFeedItemDto
                    {
                        // 🚀 تعبئة الـ dispatchId بالـ UserId الحقيقي بتاع السائق من الـ DB
                        DispatchId = driver.UserId.ToString(),
                        PhoneNumber = driver.User?.PhoneNumber ?? "No Phone In DB",
                        DriverName = driver.User != null
                            ? $"{driver.User.FirstName} {driver.User.LastName}".Trim()
                            : "No Name In DB",

                        Status = driver.Status.ToString(),
                        TripId = lastTrip?.TripId ?? 0,
                        CurrentTripId = lastTrip?.TripId ?? 0,

                        PickupZone = "",
                        DropoffZone = "",

                        StartedAt = lastTrip?.StartedAt,
                        EndedAt = lastTrip?.EndedAt,
                        DispatchedAt = lastTrip?.StartedAt ?? DateTime.UtcNow,
                        LastUpdated = lastTrip?.StartedAt ?? DateTime.UtcNow,
                        TimeElapsed = "0 mins"
                    };

                    // 3. 🚀 تعبئة الـ Zones الحقيقية بالأسماء الشغالة والمطابقة للـ Database عندك
                    if (lastTrip != null)
                    {
                        // ✨ التعديل السحري هنا: استخدام المسميات الصحيحة اللي اشتغلت في الـ StartTrip
                        item.PickupZone = lastTrip.PickupLocationId.ToString();
                        item.DropoffZone = lastTrip.DropoffLocationId.ToString();

                        if (lastTrip.EndedAt == null && lastTrip.StartedAt.HasValue)
                        {
                            var elapsed = DateTime.UtcNow - lastTrip.StartedAt.Value;
                            item.TimeElapsed = $"{(int)elapsed.TotalMinutes} mins";
                        }
                        else if (lastTrip.EndedAt.HasValue && lastTrip.StartedAt.HasValue)
                        {
                            var duration = lastTrip.EndedAt.Value - lastTrip.StartedAt.Value;
                            item.TimeElapsed = $"{(int)duration.TotalMinutes} mins (Ended)";
                        }
                    }

                    dispatchItems.Add(item);
                }

                // 4. تجهيز النتيجة النهائية النظيفة للـ Dashboard
                var result = new LiveDispatchFeedResultDto
                {
                    Items = dispatchItems,
                    TotalCount = dispatchItems.Count,
                    RetrievedAt = DateTime.UtcNow
                };

                return Result<LiveDispatchFeedResultDto>.Success(result, "Live dispatch feed retrieved successfully from database");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [DATABASE MAPPING ERROR] : {ex.Message}");
                return Result<LiveDispatchFeedResultDto>.Failure(
                    "An error occurred while pulling live data from database.",
                    "InternalServerError");
            }
        }
    }
}