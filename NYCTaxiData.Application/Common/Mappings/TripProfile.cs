using AutoMapper;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Entities;

namespace NYCTaxiData.Application.Common.Mappings
{
    public class TripProfile : Profile
    {
        public TripProfile()
        {
            CreateMap<Trip, TripDto>()
                .ForMember(dest => dest.PickupZoneName, opt => opt.MapFrom(src =>
                    src.PickupLocation != null && src.PickupLocation.Zone != null
                        ? src.PickupLocation.Zone.ZoneName
                        : "Unknown"))
                .ForMember(dest => dest.DropoffZoneName, opt => opt.MapFrom(src =>
                    src.DropoffLocation != null && src.DropoffLocation.Zone != null
                        ? src.DropoffLocation.Zone.ZoneName
                        : "Unknown"));
        }
    }
}
