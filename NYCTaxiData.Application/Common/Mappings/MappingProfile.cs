using AutoMapper;
using NYCTaxiData.Application.Auth.Commands.RegisterDriver;
using NYCTaxiData.Application.Auth.Commands.RegisterManager;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Application.Features.Trips.Commands.EndTrip;
using NYCTaxiData.Application.Features.Trips.Commands.ManualDispatch;
using NYCTaxiData.Application.Features.Trips.Commands.StartTrip;
using NYCTaxiData.Application.Features.Trips.Queries.GetLiveDispatchFeed;
using NYCTaxiData.Application.Features.Trips.Queries.GetTripHistory;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ===== RegisterDriverCommand Mapping =====
            CreateMap<RegisterDriverCommand, User1>()
                .ForMember(dest => dest.Userrole, opt => opt.MapFrom(_ => "Driver"))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<RegisterDriverCommand, Driver>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(_ => 0.0m));
            // ===== Driver Registration =====
            CreateMap<DriverRegisterDto, User1>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Userrole, opt => opt.MapFrom(_ => "Driver")) ;

            CreateMap<DriverRegisterDto, Driver>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.LicenseNumber, opt => opt.MapFrom(src => src.LicenseNumber))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(_ => 0.0m))
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            // ===== Manager Registration ===== 
            CreateMap<RegisterManagerCommand, User1>()
                .ForMember(dest => dest.Userrole, opt => opt.MapFrom(_ => "Manager"))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<RegisterManagerCommand, Manager>()
                .ForMember(dest => dest.Employeeid, opt => opt.MapFrom(src => src.EmployeeId))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department));

            // ===== Profile =====
            CreateMap<User1, ManagerProfileDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))

                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Userrole))
                .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.Department, opt => opt.Ignore());

            // في MappingProfile.cs
            CreateMap<User1, UserResultDto>()
                .ForMember(dest => dest.IsSuccess, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src =>
                    src.Driver != null ? "Driver" :
                    src.Manager != null ? "Manager" : "User"));
              
              CreateMap<RegisterDriverCommand, User1>()
             .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())  
             .ForMember(dest => dest.Userrole, opt => opt.MapFrom(src => "Driver"));
            CreateMap<User, VerifyOtpResultDto>();

            CreateMap<RegisterDriverCommand, Driver>();
            CreateMap<Driver, DriverListDto>()
            .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.UserId))

            // 👈 التوجيه الصريح للأسماء عشان AutoMapper ميتلخبطش
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName));
            CreateMap<Driver, DriverListDto>()
    .ForMember(dest => dest.DriverId, opt => opt.MapFrom(src => src.UserId.ToString()))
    .ForMember(dest => dest.FirstName,
        opt => opt.MapFrom(src => src.User != null ? src.User.FirstName : "Unknown")) // 🚀 Fallback آمن
    .ForMember(dest => dest.LastName,
        opt => opt.MapFrom(src => src.User != null ? src.User.LastName : "Unknown"))  // 🚀 Fallback آمن
    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
        }
    }
 