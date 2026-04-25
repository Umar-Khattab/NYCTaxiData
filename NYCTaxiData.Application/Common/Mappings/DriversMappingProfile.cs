using AutoMapper;
using NYCTaxiData.Application.Features.Drivers.Queries.GetActiveFleet;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;
using NYCTaxiData.Application.Features.Drivers.Queries.GetDriverProfile;
using NYCTaxiData.Domain.Entities;

namespace NYCTaxiData.Application.Common.Mappings;

public sealed class DriversMappingProfile : Profile
{
    public DriversMappingProfile()
    {
        CreateMap<Driver, DriverDto>()
            .ForMember(d => d.DriverId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Fullname ?? string.Empty))
            .ForMember(d => d.PlateNumber, opt => opt.MapFrom(s => s.Platenumber))
            .ForMember(d => d.LicenseNumber, opt => opt.MapFrom(s => s.Licensenumber))
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Driver, ActiveFleetDriverDto>()
            .ForMember(d => d.DriverId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Fullname ?? string.Empty))
            .ForMember(d => d.PlateNumber, opt => opt.MapFrom(s => s.Platenumber))
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Driver, DriverProfileDto>()
            .ForMember(d => d.DriverId, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.Fullname ?? string.Empty))
            .ForMember(d => d.PlateNumber, opt => opt.MapFrom(s => s.Platenumber))
            .ForMember(d => d.LicenseNumber, opt => opt.MapFrom(s => s.Licensenumber))
            .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PhoneNumber, opt => opt.Ignore())
            .ForMember(d => d.Email, opt => opt.Ignore())
            .ForMember(d => d.CompletedTrips, opt => opt.Ignore())
            .ForMember(d => d.ActiveTrips, opt => opt.Ignore())
            .ForMember(d => d.TotalEarnings, opt => opt.Ignore())
            .ForMember(d => d.LastTripEndedAt, opt => opt.Ignore());
    }
}
