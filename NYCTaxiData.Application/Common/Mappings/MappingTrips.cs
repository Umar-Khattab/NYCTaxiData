using AutoMapper;
using NYCTaxiData.Domain.DTOs.Identity;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Application.Features.Trips.Commands.StartTrip;
using NYCTaxiData.Application.Features.Trips.Commands.EndTrip;
using NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;
using NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Common.Mappings
{
    internal class MappingTrips : Profile
    {
        public MappingTrips()
        {
            // ===== Trips - StartTripCommand =====
            CreateMap<Trip, TripStartResultDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "In-Progress"))
                .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.DriverId.Value));

            // ===== Trips - EndTripCommand =====
            CreateMap<Trip, TripEndResultDto>()
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => 
                    src.StartedAt.HasValue && src.EndedAt.HasValue
                        ? (int)(src.EndedAt.Value - src.StartedAt.Value).TotalMinutes 
                        : 0))
                .ForMember(dest => dest.TotalFare, opt => opt.MapFrom(src => src.ActualFare ?? 0m))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Completed"));

            // ===== Trips - ManualDispatchCommand =====
            CreateMap<Trip, DispatchResultDto>()
                .ForMember(dest => dest.DispatchId, opt => opt.MapFrom(src => 
                    $"DSP-{src.TripId:D6}-{new DateTimeOffset(src.StartedAt ?? DateTime.UtcNow).ToUnixTimeSeconds()}"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Sent"))
                .ForMember(dest => dest.DispatchedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PassengerName, opt => opt.Ignore())
                .ForMember(dest => dest.PickupZoneId, opt => opt.Ignore())
                .ForMember(dest => dest.DropoffZoneId, opt => opt.Ignore());

        // ===== Trips - GetTripHistory =====
        CreateMap<Trip, TripHistoryItemDto>()
                .ForMember(dest => dest.PickupZone, opt => opt.MapFrom(src => src.PickupLocation!.Zone!.ZoneName ?? "Unknown Zone"))
                .ForMember(dest => dest.DropoffZone, opt => opt.MapFrom(src => src.DropoffLocation!.Zone!.ZoneName ?? "Unknown Zone"))
                .ForMember(dest => dest.TotalFare, opt => opt.MapFrom(src => src.ActualFare))
                .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => 
                    src.StartedAt.HasValue && src.EndedAt.HasValue
                        ? (int)(src.EndedAt.Value - src.StartedAt.Value).TotalMinutes 
                        : (src.StartedAt.HasValue? (int) (DateTime.UtcNow - src.StartedAt.Value).TotalMinutes : 0)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.EndedAt.HasValue? "Completed" : "In-Progress"));

            // ===== Trips - GetLiveDispatchFeed =====
            CreateMap<Trip, DispatchFeedItemDto>()
                .ForMember(dest => dest.DispatchId, opt => opt.MapFrom(src => 
                    $"DSP-{src.TripId:D6}-{new DateTimeOffset(src.StartedAt ?? DateTime.UtcNow).ToUnixTimeSeconds()}"))
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver!.Fullname ?? "Unknown Driver"))
                .ForMember(dest => dest.PickupZone, opt => opt.MapFrom(src => src.PickupLocation!.Zone!.ZoneName ?? "Unknown Zone"))
                .ForMember(dest => dest.DropoffZone, opt => opt.MapFrom(src => src.DropoffLocation!.Zone!.ZoneName ?? "Unknown Zone"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => DetermineDispatchStatus(src)))
                .ForMember(dest => dest.DispatchedAt, opt => opt.MapFrom(src => src.StartedAt ?? DateTime.UtcNow))
                .ForMember(dest => dest.TimeElapsed, opt => opt.MapFrom(src => FormatTimeElapsed(src.StartedAt ?? DateTime.UtcNow)));
            }

        /// <summary>
        /// Determines the current status of a dispatch based on trip state
        /// </summary>
        private static string DetermineDispatchStatus(Trip trip)
        {
            if (trip.EndedAt.HasValue)
                return "Completed";

            if (trip.StartedAt.HasValue && DateTime.UtcNow.Subtract(trip.StartedAt.Value).TotalMinutes > 60)
                return "In-Progress (Long)";

            if (trip.StartedAt.HasValue)
                return "In-Progress";

            return "Pending";
        }

        /// <summary>
        /// Formats elapsed time in human-readable format
        /// </summary>
        private static string FormatTimeElapsed(DateTime dispatchedAt)
        {
            var elapsed = DateTime.UtcNow - dispatchedAt;

            if (elapsed.TotalSeconds < 60)
                return $"{(int)elapsed.TotalSeconds} secs ago";

            if (elapsed.TotalMinutes < 60)
                return $"{(int)elapsed.TotalMinutes} mins ago";

            if (elapsed.TotalHours < 24)
                return $"{(int)elapsed.TotalHours} hours ago";

            return $"{(int)elapsed.TotalDays} days ago";
        }

    }
}
