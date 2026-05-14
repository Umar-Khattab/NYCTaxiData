using AutoMapper;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Application.Features.Drivers.Queries.GetActiveFleet;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Common.Mappings;

public sealed class DriversMappingProfile : Profile
{
    public DriversMappingProfile()
    {
        CreateMap<Driver, DriverDto>()
            .ForMember(d => d.DriverId, opt => opt.MapFrom(s => s.UserId))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName ?? string.Empty))
            .ForMember(d => d.PlateNumber, opt => opt.MapFrom(s => s.PlateNumber))
            .ForMember(d => d.LicenseNumber, opt => opt.MapFrom(s => s.LicenseNumber))
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Driver, ActiveFleetDriverDto>()
            .ForMember(d => d.DriverId, opt => opt.MapFrom(s => s.UserId))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName ?? string.Empty))
            .ForMember(d => d.PlateNumber, opt => opt.MapFrom(s => s.PlateNumber))
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Driver, DriverProfileDto>()
            .ForMember(d => d.DriverId, opt => opt.MapFrom(s => s.UserId))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FullName ?? string.Empty))
            .ForMember(d => d.PlateNumber, opt => opt.MapFrom(s => s.PlateNumber))
            .ForMember(d => d.LicenseNumber, opt => opt.MapFrom(s => s.LicenseNumber))
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PhoneNumber, opt => opt.Ignore())
            .ForMember(d => d.Email, opt => opt.Ignore())
            .ForMember(d => d.CompletedTrips, opt => opt.Ignore())
            .ForMember(d => d.ActiveTrips, opt => opt.Ignore())
            .ForMember(d => d.TotalEarnings, opt => opt.Ignore())
            .ForMember(d => d.LastTripEndedAt, opt => opt.Ignore());

        CreateMap<Driver, DriverListDto>()
         .ForMember(dest => dest.DriverId,
             opt => opt.MapFrom(src => src.UserId.ToString()))
         .ForMember(dest => dest.FirstName,
             opt => opt.MapFrom(src =>
                 src.User != null ? src.User.FirstName : "Unknown")) // ✅
         .ForMember(dest => dest.LastName,
             opt => opt.MapFrom(src =>
                 src.User != null ? src.User.LastName : "Unknown"))  // ✅
         .ForMember(dest => dest.Status,
             opt => opt.MapFrom(src => src.Status.ToString()));
    }
    }
