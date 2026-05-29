using AutoMapper;
using NYCTaxiData.Application.DTOs.Zones;
using NYCTaxiData.Domain.Entities;

namespace NYCTaxiData.Application.Common.Mappings;

public sealed class ZoneProfile : Profile
{
    public ZoneProfile()
    {
        CreateMap<Zone, ZoneDto>();
    }
}
