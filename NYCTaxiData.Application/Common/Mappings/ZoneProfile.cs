using AutoMapper;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Entities;

namespace NYCTaxiData.Application.Common.Mappings
{
    public class ZoneProfile : Profile
    {
        public ZoneProfile()
        {
            CreateMap<Zone, ZoneDto>();
            CreateMap<Zone, ZoneStatisticsDto>()
                .ForMember(dest => dest.TotalPickupTrips, opt => opt.Ignore())
                .ForMember(dest => dest.TotalDropoffTrips, opt => opt.Ignore())
                .ForMember(dest => dest.TotalRevenue, opt => opt.Ignore())
                .ForMember(dest => dest.AvgFare, opt => opt.Ignore())
                .ForMember(dest => dest.AvgTip, opt => opt.Ignore())
                .ForMember(dest => dest.BusiestHourOfDay, opt => opt.Ignore())
                .ForMember(dest => dest.BusiestDayOfWeek, opt => opt.Ignore());
        }
    }
}
