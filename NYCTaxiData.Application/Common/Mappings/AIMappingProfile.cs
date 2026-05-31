using AutoMapper;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.EntitiesAi;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Infrastructure.Domain.EntitiesAi;

namespace NYCTaxiData.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for all AI-related mappings:
/// - DB Entities → Prediction Input DTOs
/// - Prediction Input DTOs → ML API Request Rows
/// - Zone Entity → ZoneSupplyState
/// - Simulation Entities → Response DTOs
/// </summary>
public sealed class AIMappingProfile : Profile
{
    public AIMappingProfile()
    {
        ConfigureDemand15MinMappings();
        ConfigureDemand6hMappings();
        ConfigureETAMappings();
        ConfigureRevenueMappings();
        ConfigureStockOutMappings();
        ConfigureZoneMappings();
    }

    // =========================================================
    // 1. Demand 15-Min (تعديل الحرف الأول لـ سمول في الـ Constructor Parameters)
    // =========================================================
    private void ConfigureDemand15MinMappings()
    {
        // 🚀 الحركة الصايعة: بنقول للـ AutoMapper قسماً بالله ما تقرب للـ Constructor واعمل مابينج للحقول عل طول
        CreateMap<Demand15min, Demand15MinInput>(MemberList.None)
            .DisableCtorValidation() // 👈 بتلغي فحص الـ Constructor Parameters تماماً لمنع الـ Exception نهائياً
            .ForMember(dest => dest.ZoneId, opt => opt.MapFrom(src => src.PuLocationId ?? 0))
            .ForMember(dest => dest.Hour, opt => opt.MapFrom(src => src.Hour ?? 0))
            .ForMember(dest => dest.Minute, opt => opt.MapFrom(src => src.Minute ?? 0))
            .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek ?? 0))
            .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.Month ?? 1))
            .ForMember(dest => dest.IsWeekend, opt => opt.MapFrom(src => src.IsWeekend == 1))
            .ForMember(dest => dest.Lag1, opt => opt.MapFrom(src => (double)(src.Lag1 ?? 0)))
            .ForMember(dest => dest.Lag4, opt => opt.MapFrom(src => (double)(src.Lag4 ?? 0)))
            .ForMember(dest => dest.Lag96, opt => opt.MapFrom(src => (double)(src.Lag96 ?? 0)))
            .ForMember(dest => dest.RollMean1h, opt => opt.MapFrom(src => (double)(src.RollMean1h ?? 0)))
            .ForMember(dest => dest.RollMean3h, opt => opt.MapFrom(src => (double)(src.RollMean3h ?? 0)))
            .ForMember(dest => dest.TempC, opt => opt.MapFrom(src => (double)(src.TempC ?? 0)))
            .ForMember(dest => dest.RainMm, opt => opt.MapFrom(src => (double)(src.RainMm ?? 0)))
            .ForMember(dest => dest.IsRain, opt => opt.MapFrom(src => src.IsRain == 1))
            .ForMember(dest => dest.WeatherCode, opt => opt.MapFrom(src => src.WeatherCode ?? 0));
    }

    // =========================================================
    // 2. Demand 6h
    // =========================================================
    private void ConfigureDemand6hMappings()
    {
        CreateMap<Demandfeature, Demand6hInput>(MemberList.None)
            .DisableCtorValidation() // 👈 بتلغي فحص الـ Constructor هنا كمان
            .ForMember(dest => dest.ZoneId, opt => opt.MapFrom(src => src.PuLocationId ?? 0))
            .ForMember(dest => dest.PickupHour, opt => opt.MapFrom(src => src.PickupHour ?? 0))
            .ForMember(dest => dest.DayOfWeek, opt => opt.MapFrom(src => src.DayOfWeek ?? 0))
            .ForMember(dest => dest.IsWeekend, opt => opt.MapFrom(src => src.IsWeekend == 1))
            .ForMember(dest => dest.IsHoliday, opt => opt.MapFrom(src => src.IsHoliday == 1))
            .ForMember(dest => dest.Lag1_6h, opt => opt.MapFrom(src => (double)(src.Lag16h ?? 0)))
            .ForMember(dest => dest.Lag2_6h, opt => opt.MapFrom(src => (double)(src.Lag26h ?? 0)))
            .ForMember(dest => dest.Lag4_6h, opt => opt.MapFrom(src => (double)(src.Lag46h ?? 0)))
            .ForMember(dest => dest.RollingMean24h, opt => opt.MapFrom(src => (double)(src.RollingMean24h ?? 0)))
            .ForMember(dest => dest.TempC, opt => opt.MapFrom(src => (double)(src.TempC ?? 0)))
            .ForMember(dest => dest.RainMm, opt => opt.MapFrom(src => (double)(src.RainMm ?? 0)))
            .ForMember(dest => dest.IsRain, opt => opt.MapFrom(src => src.IsRain == 1))
            .ForMember(dest => dest.WeatherCode, opt => opt.MapFrom(src => src.WeatherCode ?? 0));
    }
    // =========================================================
    // 3. ETA
    // =========================================================
    private void ConfigureETAMappings()
    {
        CreateMap<Etum, ETAInput>()
            .ForCtorParam("PickupZoneId",
                opt => opt.MapFrom(src => src.PuLocationId ?? 0))
            .ForCtorParam("DropoffZoneId",
                opt => opt.MapFrom(src => src.DoLocationId ?? 0))
            .ForCtorParam("TempC",
                opt => opt.MapFrom(src => src.TempC))
            .ForCtorParam("RainMm",
                opt => opt.MapFrom(src => src.RainMm))
            .ForCtorParam("WeatherCode",
                opt => opt.MapFrom(src => src.WeatherCode))
            .ForCtorParam("DistanceProxy",
                opt => opt.MapFrom(src => src.DistanceProxy ?? 0m))
            .ForCtorParam("PUHour",
                opt => opt.MapFrom(src => src.PickupHour ?? 0))
            .ForCtorParam("PUDow",
                opt => opt.MapFrom(src => src.PickupDow ?? 0))
            .ForCtorParam("PUMonth",
                opt => opt.MapFrom(src => src.PickupMonth ?? 0))
            .ForCtorParam("PUMinute",
                opt => opt.MapFrom(src => src.PickupMinute ?? 0))
            .ForCtorParam("IsWeekend",
                opt => opt.MapFrom(src => src.IsWeekend ?? 0))
            .ForCtorParam("IsRushHour",
                opt => opt.MapFrom(src => src.IsRushHour ?? 0))
            .ForCtorParam("PU15MinBucket",
                opt => opt.MapFrom(src => src.Pickup15minBucket ?? DateTime.UtcNow))
            .ForCtorParam("DistanceBucketLabel",
                opt => opt.MapFrom(src => src.DistanceBucketLabel ?? ""))
            .ForCtorParam("DurationSec",
                opt => opt.MapFrom(src => src.DurationSec ?? 0m))
            .ForCtorParam("OdHourMedianDuration",
                opt => opt.MapFrom(src => src.OdHourMedianDuration ?? 0m))
            .ForCtorParam("PUHourSlowdownIndex",
                opt => opt.MapFrom(src => src.PuHourSlowdownIndex ?? 0m))
            .ForCtorParam("DistMedianDuration",
                opt => opt.MapFrom(src => src.DistMedianDuration ?? 0));
    }

    // =========================================================
    // 4. Revenue
    // =========================================================
    private void ConfigureRevenueMappings()
    {
        CreateMap<Revenuefeature, RevenueInput>()
            .ForCtorParam("ZoneId",
                opt => opt.MapFrom(src => src.PuLocationId ?? 0))
            .ForCtorParam("PickupHour",
                opt => opt.MapFrom(src => src.PickupHour ?? 0))
            .ForCtorParam("DayOfWeek",
                opt => opt.MapFrom(src => src.DayOfWeek ?? 0))
            .ForCtorParam("IsWeekend",
                opt => opt.MapFrom(src => src.IsWeekend == 1))
            .ForCtorParam("lag1_6h",
                opt => opt.MapFrom(src => src.Lag16h ?? 0))
            .ForCtorParam("lag2_6h",
                opt => opt.MapFrom(src => src.Lag26h ?? 0))
            .ForCtorParam("lag4_6h",
                opt => opt.MapFrom(src => src.Lag46h ?? 0))
            .ForCtorParam("RevLag1_6h",
                opt => opt.MapFrom(src => (double)(src.RevLag16h ?? 0)))
            .ForCtorParam("RevLag1Week",
                opt => opt.MapFrom(src => (double)(src.RevLag1Week ?? 0)))
            .ForCtorParam("RevRollingMean7d",
                opt => opt.MapFrom(src => (double)(src.RevRollingMean7d ?? 0)))
            .ForCtorParam("RevRollingMean30d",
                opt => opt.MapFrom(src => (double)(src.RevRollingMean30d ?? 0)))
            .ForCtorParam("RollingMean24h",
                opt => opt.MapFrom(src => src.RollingMean24h))
            .ForCtorParam("AvgFare",
                opt => opt.MapFrom(src => (double)(src.AvgFare ?? 0)))
            .ForCtorParam("TipRate",
                opt => opt.MapFrom(src => (double)(src.TipRate ?? 0)))
            .ForCtorParam("TempC",
                opt => opt.MapFrom(src => (double?)src.TempC))
            .ForCtorParam("RainMm",
                opt => opt.MapFrom(src => (double?)src.RainMm))
            .ForCtorParam("IsRain",
                opt => opt.MapFrom(src => src.IsRain == 1 ? (bool?)true : false))
            .ForCtorParam("WeatherCode",
                opt => opt.MapFrom(src => src.WeatherCode))
            .ForCtorParam("IsHoliday",
                opt => opt.MapFrom(src => src.IsHoliday == 1 ? (bool?)true : false));
    }

    // =========================================================
    // 5. StockOut
    // =========================================================
    private void ConfigureStockOutMappings()
    {
        CreateMap<Stockoutfeature, StockOutInput>()
            .ForCtorParam("ZoneId",
                opt => opt.MapFrom(src => src.ZoneId ?? 0))
            .ForCtorParam("TimeBucket6h",
                opt => opt.MapFrom(src => src.TimeBucket6h ?? DateTime.UtcNow))
            .ForCtorParam("PickupCount",
                opt => opt.MapFrom(src => (double)(src.PickupCount ?? 0)))
            .ForCtorParam("DropoffCount",
                opt => opt.MapFrom(src => (double)(src.DropoffCount ?? 0)))
            .ForCtorParam("NetFlow",
                opt => opt.MapFrom(src => (double)(src.NetFlow ?? 0)))
            .ForCtorParam("Hour",
                opt => opt.MapFrom(src => src.Hour ?? 0))
            .ForCtorParam("DayOfWeek",
                opt => opt.MapFrom(src => src.DayOfWeek ?? 0))
            .ForCtorParam("IsWeekend",
                opt => opt.MapFrom(src => src.IsWeekend == 1))
            .ForCtorParam("IsHoliday",
                opt => opt.MapFrom(src => src.IsHoliday == 1))
            .ForCtorParam("ActivityRatio",
                opt => opt.MapFrom(src => (double)(src.ActivityRatio ?? 0)))
            .ForCtorParam("TempC",
                opt => opt.MapFrom(src => (double)(src.TempC ?? 0)))
            .ForCtorParam("RainMm",
                opt => opt.MapFrom(src => (double)(src.RainMm ?? 0)))
            .ForCtorParam("IsRain",
                opt => opt.MapFrom(src => src.IsRain == 1))
            .ForCtorParam("Lag1Pickup",
                opt => opt.MapFrom(src => (double)(src.Lag1Pickup ?? 0)))
            .ForCtorParam("Lag1Dropoff",
                opt => opt.MapFrom(src => (double)(src.Lag1Dropoff ?? 0)))
            .ForCtorParam("Lag1NetFlow",
                opt => opt.MapFrom(src => (double)(src.Lag1NetFlow ?? 0)))
            .ForCtorParam("WeatherCode",
                opt => opt.MapFrom(src => src.WeatherCode ?? 0));
    }

    // =========================================================
    // 6. Zone → ZoneSupplyState
    // =========================================================
    private void ConfigureZoneMappings()
    {
        CreateMap<Zone, ZoneSupplyState>()
            .ForCtorParam("ZoneId",
                opt => opt.MapFrom(src => src.ZoneId))
            .ForCtorParam("CurrentSupply",
                opt => opt.MapFrom(src => 0))
            .ForCtorParam("ActiveTrips",
                opt => opt.MapFrom(src => 0))
            .ForCtorParam("ForecastedDemand",
                opt => opt.MapFrom(src => (double?)null))
            .ForCtorParam("StockOutRisk",
                opt => opt.MapFrom(src => (double?)null))
            .ForCtorParam("ExpectedRevenue",
                opt => opt.MapFrom(src => (double?)null));
    }
}
