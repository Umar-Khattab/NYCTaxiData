using AutoMapper;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Application.Features.Trips.Commands.CreateTrip;
using NYCTaxiData.Application.Features.Trips.Commands.UpdateTrip;
using NYCTaxiData.Domain.Entities;

namespace NYCTaxiData.Application.Common.Mappings;

public sealed class TripProfile : Profile
{
    public TripProfile()
    {
        CreateMap<Trip, TripDto>();
        CreateMap<Trip, TripDeleteResultDto>();

        CreateMap<CreateTripCommand, Trip>()
            .ForMember(dest => dest.TripId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.PickupLocation, opt => opt.Ignore())
            .ForMember(dest => dest.DropoffLocation, opt => opt.Ignore());

        CreateMap<UpdateTripCommand, Trip>()
            .ForMember(dest => dest.TripId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Driver, opt => opt.Ignore())
            .ForMember(dest => dest.PickupLocation, opt => opt.Ignore())
            .ForMember(dest => dest.DropoffLocation, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((_, __, srcMember) => srcMember is not null));
    }
}
