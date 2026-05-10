using AutoMapper;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure;
using System;

namespace NYCTaxiData.Application.Common.Mappings
{
    public class MappingTrips : Profile
    {
        public MappingTrips()
        {
            // ===== Drivers =====
            CreateMap<Driver, DriverListDto>()
                .ForMember(d => d.DriverId, o => o.MapFrom(s => s.UserId)) // تعديل لـ UserId
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FullName)) // تعديل لـ FullName
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // ===== Trips - StartTripResultDto =====
            CreateMap<Trip, DTOs.Trip.TripStartResultDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "In-Progress"))
                .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.DriverId)); // DriverId هو Guid?

            // ===== Trips - EndTripResultDto =====
            CreateMap<Trip, DTOs.Trip.TripEndResultDto>()
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src =>
                    src.EndedAt.HasValue
                        ? (int)(src.EndedAt.Value - src.StartedAt).TotalMinutes // StartedAt ليست Nullable
                        : 0))
                .ForMember(dest => dest.TotalFare, opt => opt.MapFrom(src => src.TotalAmount ?? 0m)) // تعديل لـ TotalAmount
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Completed"));

            // ===== Trips - DispatchResultDto =====
            CreateMap<Trip, DTOs.Trip.DispatchResultDto>()
                .ForMember(dest => dest.DispatchId, opt => opt.MapFrom(src =>
                    $"DSP-{src.TripId:D6}-{new DateTimeOffset(src.StartedAt).ToUnixTimeSeconds()}")) // StartedAt ليست Nullable
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Sent"))
                .ForMember(dest => dest.DispatchedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ===== Trips - GetTripHistory =====
            CreateMap<Trip, TripHistoryItemDto>()
                .ForMember(dest => dest.PickupZone, opt => opt.MapFrom(src => src.PickupLocation!.Zone!.ZoneName ?? "Unknown Zone"))
                .ForMember(dest => dest.DropoffZone, opt => opt.MapFrom(src => src.DropoffLocation!.Zone!.ZoneName ?? "Unknown Zone"))
                .ForMember(dest => dest.TotalFare, opt => opt.MapFrom(src => src.TotalAmount)) // تعديل لـ TotalAmount
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src =>
                    src.EndedAt.HasValue
                        ? (int)(src.EndedAt.Value - src.StartedAt).TotalMinutes
                        : (int)(DateTime.UtcNow - src.StartedAt).TotalMinutes))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.EndedAt.HasValue ? "Completed" : "In-Progress"));

            // ===== Trips - GetLiveDispatchFeed =====
            CreateMap<Trip, DTOs.Trip.DispatchFeedItemDto>()
                .ForMember(dest => dest.DispatchId, opt => opt.MapFrom(src =>
                    $"DSP-{src.TripId:D6}-{new DateTimeOffset(src.StartedAt).ToUnixTimeSeconds()}"))
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver!.FullName ?? "Unknown Driver")) // FullName
                .ForMember(dest => dest.PickupZone, opt => opt.MapFrom(src => src.PickupLocation!.Zone!.ZoneName ?? "Unknown Zone"))
                .ForMember(dest => dest.DropoffZone, opt => opt.MapFrom(src => src.DropoffLocation!.Zone!.ZoneName ?? "Unknown Zone"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DetermineDispatchStatus(src)))
                .ForMember(dest => dest.DispatchedAt, opt => opt.MapFrom(src => src.StartedAt))
                .ForMember(dest => dest.TimeElapsed, opt => opt.MapFrom(src => FormatTimeElapsed(src.StartedAt)));
        }

        private static string DetermineDispatchStatus(Trip trip)
        {
            if (trip.EndedAt.HasValue)
                return "Completed";

            if (DateTime.UtcNow.Subtract(trip.StartedAt).TotalMinutes > 60)
                return "In-Progress (Long)";

            return "In-Progress";
        }

        private static string FormatTimeElapsed(DateTime dispatchedAt)
        {
            var elapsed = DateTime.UtcNow - dispatchedAt;
            if (elapsed.TotalSeconds < 60) return $"{(int)elapsed.TotalSeconds} secs ago";
            if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes} mins ago";
            if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours} hours ago";
            return $"{(int)elapsed.TotalDays} days ago";
        }
    }
}