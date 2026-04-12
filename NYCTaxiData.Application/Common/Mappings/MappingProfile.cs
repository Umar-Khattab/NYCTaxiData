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
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ===== Driver Registration =====
            CreateMap<DriverRegisterDto, User1>()
                .ForMember(dest => dest.Firstname, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.Lastname, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Phonenumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Passwordhash, opt => opt.Ignore())
                .ForMember(dest => dest.Createdat, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore());

            CreateMap<DriverRegisterDto, Driver>()
                .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Licensenumber, opt => opt.MapFrom(src => src.LicenseNumber))
                .ForMember(dest => dest.Platenumber, opt => opt.MapFrom(src => src.PlateNumber))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(_ => 0.0m))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // ===== Manager Registration =====
            CreateMap<ManagerRegisterDto, Manager>()
                .ForMember(dest => dest.Employeeid, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department))
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // ===== Profile =====
            CreateMap<User1, ManagerProfileDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Firstname))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Lastname))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore());

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
                        : (src.StartedAt.HasValue ? (int)(DateTime.UtcNow - src.StartedAt.Value).TotalMinutes : 0)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.EndedAt.HasValue ? "Completed" : "In-Progress"));

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
