using AutoMapper;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Infrastructure.Domain.EntitiesAi;
using NYCTaxiData.Domain.EntitiesAi;

namespace NYCTaxiData.Application.Features.AI;

/// <summary>
/// AutoMapper profile for AI feature mappings between entities and DTOs.
/// </summary>
public class AIMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AIMappingProfile"/> class.
    /// </summary>
    public AIMappingProfile()
    {
        // Demand prediction mappings
        CreateMap<Demandfeature, Demand6hInput>()
            .ForMember(dest => dest.ZoneId, opt => opt.MapFrom(src => src.PuLocationId))
            .ForMember(dest => dest.PickupHour, opt => opt.MapFrom(src => src.PickupHour))
            .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
            .ForMember(dest => dest.IsWeekend, opt => opt.MapFrom(src => src.IsWeekend == 1))
            .ForMember(dest => dest.TempC, opt => opt.MapFrom(src => src.TempC ?? 0))
            .ForMember(dest => dest.RainMm, opt => opt.MapFrom(src => src.RainMm ?? 0))
            .ForMember(dest => dest.IsRain, opt => opt.MapFrom(src => src.IsRain == 1))
            .ForMember(dest => dest.WeatherCode, opt => opt.MapFrom(src => src.WeatherCode ?? 0))
            .ForMember(dest => dest.Lag1_6h, opt => opt.MapFrom(src => src.Lag16h ?? 0))
            .ForMember(dest => dest.Lag2_6h, opt => opt.MapFrom(src => src.Lag26h ?? 0))
            .ForMember(dest => dest.Lag4_6h, opt => opt.MapFrom(src => src.Lag46h ?? 0))
            .ForMember(dest => dest.IsHoliday, opt => opt.MapFrom(src => src.IsHoliday ?? 1))
            .ForMember(dest => dest.PickupCount, opt => opt.MapFrom(src => src.PickupCount ?? 0))
            .ForMember(dest => dest.RollingMean24h, opt => opt.MapFrom(src => src.RollingMean24h ?? 0));

        CreateMap<Demand15min, Demand15MinInput>()
            .ForMember(dest => dest.ZoneId, opt => opt.MapFrom(src => src.PuLocationId))
            .ForMember(dest => dest.Hour, opt => opt.MapFrom(src => src.Hour))
            .ForMember(dest => dest.Minute, opt => opt.MapFrom(src => src.Minute))
            .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
            .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.Month))
            .ForMember(dest => dest.IsWeekend, opt => opt.MapFrom(src => src.IsWeekend ?? 0))
            .ForMember(dest => dest.Lag1, opt => opt.MapFrom(src => src.Lag1 ?? 0))
            .ForMember(dest => dest.Lag4, opt => opt.MapFrom(src => src.Lag4 ?? 0))
            .ForMember(dest => dest.Lag96, opt => opt.MapFrom(src => src.Lag96 ?? 0))
            .ForMember(dest => dest.RollMean1h, opt => opt.MapFrom(src => src.RollMean1h ?? 0))
            .ForMember(dest => dest.RollMean3h, opt => opt.MapFrom(src => src.RollMean3h ?? 0))
            .ForMember(dest => dest.TempC, opt => opt.MapFrom(src => src.TempC ?? 0))
            .ForMember(dest => dest.RainMm, opt => opt.MapFrom(src => src.RainMm ?? 0))
            .ForMember(dest => dest.IsRain, opt => opt.MapFrom(src => src.IsRain ?? 0))
            .ForMember(dest => dest.WeatherCode, opt => opt.MapFrom(src => src.WeatherCode ?? 0))
            .ForMember(dest => dest.PickupCount, opt => opt.MapFrom(src => src.PickupCnt ?? 0));

        CreateMap<Etum, ETAInput>()
            .ForMember(dest => dest.PickupZoneId, opt => opt.MapFrom(src => src.PuLocationId))
            .ForMember(dest => dest.DropoffZoneId, opt => opt.MapFrom(src => src.DoLocationId))
            .ForMember(dest => dest.TempC, opt => opt.MapFrom(src => src.TempC ?? 0))
            .ForMember(dest => dest.RainMm, opt => opt.MapFrom(src => src.RainMm ?? 0))
            .ForMember(dest => dest.WeatherCode, opt => opt.MapFrom(src => src.WeatherCode ?? 0))
            .ForMember(dest => dest.DistanceProxy, opt => opt.MapFrom(src => src.DistanceProxy ?? 0))
            .ForMember(dest => dest.PUHour, opt => opt.MapFrom(src => src.PickupHour ?? 0))
            .ForMember(dest => dest.PUDow, opt => opt.MapFrom(src => src.PickupDow ?? 0))
            .ForMember(dest => dest.PUMonth, opt => opt.MapFrom(src => src.PickupMonth ?? 0))
            .ForMember(dest => dest.PUMinute, opt => opt.MapFrom(src => src.PickupMinute ?? 0))
            .ForMember(dest => dest.IsWeekend, opt => opt.MapFrom(src => src.IsWeekend ?? 0))
            .ForMember(dest => dest.IsRushHour, opt => opt.MapFrom(src => src.IsRushHour ?? 0))
            .ForMember(dest => dest.PU15MinBucket, opt => opt.MapFrom(src => src.Pickup15minBucket ?? DateTime.MinValue))
            .ForMember(dest => dest.DistanceBucketLabel, opt => opt.MapFrom(src => src.DistanceBucketLabel ?? "Unknown"))
            .ForMember(dest => dest.DurationSec, opt => opt.MapFrom(src => src.DurationSec ?? 0))
            .ForMember(dest => dest.OdHourMedianDuration, opt => opt.MapFrom(src => src.OdHourMedianDuration ?? 0))
            .ForMember(dest => dest.PUHourSlowdownIndex, opt => opt.MapFrom(src => src.PuHourSlowdownIndex ?? 0))
            .ForMember(dest => dest.DistMedianDuration, opt => opt.MapFrom(src => src.DistMedianDuration ?? 0));

        CreateMap<Revenuefeature, RevenueInput>()
            .ForMember(dest => dest.ZoneId, opt => opt.MapFrom(src => src.PuLocationId))
            .ForMember(dest => dest.PickupHour, opt => opt.MapFrom(src => src.PickupHour))
            .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek))
            .ForMember(dest => dest.IsWeekend, opt => opt.MapFrom(src => src.IsWeekend ?? 0))
            .ForMember(dest => dest.lag1_6h, opt => opt.MapFrom(src => src.Lag16h ?? 0))
            .ForMember(dest => dest.lag2_6h, opt => opt.MapFrom(src => src.Lag26h ?? 0))
            .ForMember(dest => dest.lag4_6h, opt => opt.MapFrom(src => src.Lag46h ?? 0))
            .ForMember(dest => dest.RevLag1_6h, opt => opt.MapFrom(src => src.RevLag16h ?? 0))
            .ForMember(dest => dest.RevLag1Week, opt => opt.MapFrom(src => src.RevLag1Week ?? 0))
            .ForMember(dest => dest.RevRollingMean7d, opt => opt.MapFrom(src => src.RevRollingMean7d ?? 0))
            .ForMember(dest => dest.RevRollingMean30d, opt => opt.MapFrom(src => src.RevRollingMean30d ?? 0))
            .ForMember(dest => dest.RollingMean24h, opt => opt.MapFrom(src => src.RollingMean24h ?? 0))
            .ForMember(dest => dest.TempC, opt => opt.MapFrom(src => src.TempC ?? 0))
            .ForMember(dest => dest.RainMm, opt => opt.MapFrom(src => src.RainMm ?? 0))
            .ForMember(dest => dest.IsRain, opt => opt.MapFrom(src => src.IsRain ?? 0))
            .ForMember(dest => dest.IsHoliday, opt => opt.MapFrom(src => src.IsHoliday ?? 0))
            .ForMember(dest => dest.AvgFare, opt => opt.MapFrom(src => src.AvgFare ?? 0))
            .ForMember(dest => dest.TipRate, opt => opt.MapFrom(src => src.TipRate ?? 0))
            .ForMember(dest => dest.WeatherCode, opt => opt.MapFrom(src => src.WeatherCode ?? 0));

        CreateMap<Stockoutfeature, StockOutInput>()
            .ForMember(dest => dest.ZoneId, opt => opt.MapFrom(src => src.ZoneId))
            .ForMember(dest => dest.TimeBucket6h, opt => opt.MapFrom(src => src.TimeBucket6h ?? DateTime.MinValue))
            .ForMember(dest => dest.PickupCount, opt => opt.MapFrom(src => src.PickupCount ?? 0))
            .ForMember(dest => dest.DropoffCount, opt => opt.MapFrom(src => src.DropoffCount ?? 0))
            .ForMember(dest => dest.NetFlow, opt => opt.MapFrom(src => src.NetFlow ?? 0))
            .ForMember(dest => dest.Hour, opt => opt.MapFrom(src => src.Hour ?? 0))
            .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek ?? 0))
            .ForMember(dest => dest.IsWeekend, opt => opt.MapFrom(src => src.IsWeekend ?? 0))
            .ForMember(dest => dest.IsHoliday, opt => opt.MapFrom(src => src.IsHoliday ?? 0))
            .ForMember(dest => dest.ActivityRatio, opt => opt.MapFrom(src => src.ActivityRatio ?? 0))
            .ForMember(dest => dest.TempC, opt => opt.MapFrom(src => src.TempC ?? 0))
            .ForMember(dest => dest.RainMm, opt => opt.MapFrom(src => src.RainMm ?? 0))
            .ForMember(dest => dest.IsRain, opt => opt.MapFrom(src => src.IsRain ?? 0))
            .ForMember(dest => dest.WeatherCode, opt => opt.MapFrom(src => src.WeatherCode ?? 0))
            .ForMember(dest => dest.Lag1Pickup, opt => opt.MapFrom(src => src.Lag1Pickup ?? 0))
            .ForMember(dest => dest.Lag1Dropoff, opt => opt.MapFrom(src => src.Lag1Dropoff ?? 0))
            .ForMember(dest => dest.Lag1NetFlow, opt => opt.MapFrom(src => src.Lag1NetFlow ?? 0));

        // Zone supply state mappings
        CreateMap<Zone, ZoneSupplyState>()
            .ForMember(dest => dest.CurrentSupply, opt => opt.Ignore())
            .ForMember(dest => dest.ActiveTrips, opt => opt.Ignore())
            .ForMember(dest => dest.ForecastedDemand, opt => opt.Ignore())
            .ForMember(dest => dest.StockOutRisk, opt => opt.Ignore())
            .ForMember(dest => dest.ExpectedRevenue, opt => opt.Ignore());
    }
}
