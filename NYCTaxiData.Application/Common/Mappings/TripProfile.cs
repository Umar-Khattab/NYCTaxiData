using AutoMapper;
using NYCTaxiData.Application.DTOs.Trip;
using NYCTaxiData.Domain.Entities;

namespace NYCTaxiData.Application.Common.Mappings
{
    public class TripProfile : Profile
    {
        public TripProfile()
        {
            // 🚀 التعديل العبقري: تأمين الـ TripStartResultDto عشان لو الـ Entity فيها Null والـ Handler ماليها، الـ AutoMapper ميعملش Overwrite بـ Null
            CreateMap<Trip, TripStartResultDto>()
                .ForMember(dest => dest.PickupLocationId, opt => opt.MapFrom(src => src.PickupLocationId))
                .ForMember(dest => dest.DropoffLocationId, opt => opt.MapFrom(src => src.DropoffLocationId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Ongoing"));

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