using AutoMapper;
using NYCTaxiData.Application.DTOs.Identity;
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

            // في MappingProfile.cs
            CreateMap<User1, UserResultDto>()
                .ForMember(dest => dest.IsSuccess, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.Firstname} {src.Lastname}"))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src =>
                    src.Driver != null ? "Driver" :
                    src.Manager != null ? "Manager" : "User"));
        }
    }
}