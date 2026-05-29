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
        //ConfigureZoneMappings();
        //ConfigureSimulationMappings();
    }

    // =========================================================
    // 1. Demand 15-Min
    // =========================================================
    private void ConfigureDemand15MinMappings()
    {
        // DB Entity → Input DTO
        CreateMap<Demand15min, Demand15MinInput>()
            .ForCtorParam("ZoneId",
                opt => opt.MapFrom(src => src.PuLocationId ?? 0))
            .ForCtorParam("Hour",
                opt => opt.MapFrom(src => src.Hour ?? 0))
            .ForCtorParam("Minute",
                opt => opt.MapFrom(src => src.Minute ?? 0))
            .ForCtorParam("DayOfWeek",
                opt => opt.MapFrom(src => src.DayOfWeek ?? 0))
            .ForCtorParam("Month",
                opt => opt.MapFrom(src => src.Month ?? 1))
            .ForCtorParam("IsWeekend",
                opt => opt.MapFrom(src => src.IsWeekend == 1))
            .ForCtorParam("Lag1",
                opt => opt.MapFrom(src => (double)(src.Lag1 ?? 0)))
            .ForCtorParam("Lag4",
                opt => opt.MapFrom(src => (double)(src.Lag4 ?? 0)))
            .ForCtorParam("Lag96",
                opt => opt.MapFrom(src => (double)(src.Lag96 ?? 0)))
            .ForCtorParam("RollMean1h",
                opt => opt.MapFrom(src => (double)(src.RollMean1h ?? 0)))
            .ForCtorParam("RollMean3h",
                opt => opt.MapFrom(src => (double)(src.RollMean3h ?? 0)))
            .ForCtorParam("TempC",
                opt => opt.MapFrom(src => (double)(src.TempC ?? 0)))
            .ForCtorParam("RainMm",
                opt => opt.MapFrom(src => (double)(src.RainMm ?? 0)))
            .ForCtorParam("IsRain",
                opt => opt.MapFrom(src => src.IsRain == 1))
            .ForCtorParam("WeatherCode",
                opt => opt.MapFrom(src => src.WeatherCode ?? 0));
    }

    // =========================================================
    // 2. Demand 6h
    // =========================================================
    private void ConfigureDemand6hMappings()
    {
        // DB Entity → Input DTO
        CreateMap<Demandfeature, Demand6hInput>()
            .ForCtorParam("ZoneId",
                opt => opt.MapFrom(src => src.PuLocationId ?? 0))
            .ForCtorParam("PickupHour",
                opt => opt.MapFrom(src => src.PickupHour ?? 0))
            .ForCtorParam("DayOfWeek",
                opt => opt.MapFrom(src => src.DayOfWeek ?? 0))
            .ForCtorParam("IsWeekend",
                opt => opt.MapFrom(src => src.IsWeekend == 1))
            .ForCtorParam("IsHoliday",
                opt => opt.MapFrom(src => src.IsHoliday == 1))
            .ForCtorParam("Lag1_6h",
                opt => opt.MapFrom(src => (double)(src.Lag16h ?? 0)))
            .ForCtorParam("Lag2_6h",
                opt => opt.MapFrom(src => (double)(src.Lag26h ?? 0)))
            .ForCtorParam("Lag4_6h",
                opt => opt.MapFrom(src => (double)(src.Lag46h ?? 0)))
            .ForCtorParam("RollingMean24h",
                opt => opt.MapFrom(src => (double)(src.RollingMean24h ?? 0)))
            .ForCtorParam("TempC",
                opt => opt.MapFrom(src => (double)(src.TempC ?? 0)))
            .ForCtorParam("RainMm",
                opt => opt.MapFrom(src => (double)(src.RainMm ?? 0)))
            .ForCtorParam("IsRain",
                opt => opt.MapFrom(src => src.IsRain == 1))
            .ForCtorParam("WeatherCode",
                opt => opt.MapFrom(src => src.WeatherCode ?? 0));
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
            .ForCtorParam("PickupDateTime",
                opt => opt.MapFrom(src => src.Pickup15minBucket ?? DateTime.UtcNow))
            .ForCtorParam("TripDistance",
                opt => opt.MapFrom(src => (double)(src.DistanceProxy ?? 0)))
            .ForCtorParam("TempC",
                opt => opt.MapFrom(src => (double?)src.TempC))
            .ForCtorParam("RainMm",
                opt => opt.MapFrom(src => (double?)src.RainMm))
            .ForCtorParam("WeatherCode",
                opt => opt.MapFrom(src => src.WeatherCode));
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
            .ForCtorParam("ForecastedDemand6h",
                opt => opt.MapFrom(src => (double)(src.Lag16h ?? 0)))
            .ForCtorParam("RevLag1_6h",
                opt => opt.MapFrom(src => (double)(src.RevLag16h ?? 0)))
            .ForCtorParam("RevLag1Week",
                opt => opt.MapFrom(src => (double)(src.RevLag1Week ?? 0)))
            .ForCtorParam("RevRollingMean7d",
                opt => opt.MapFrom(src => (double)(src.RevRollingMean7d ?? 0)))
            .ForCtorParam("RevRollingMean30d",
                opt => opt.MapFrom(src => (double)(src.RevRollingMean30d ?? 0)))
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
            .ForCtorParam("Hour",
                opt => opt.MapFrom(src => src.Hour ?? 0))
            .ForCtorParam("DayOfWeek",
                opt => opt.MapFrom(src => src.DayOfWeek ?? 0))
            .ForCtorParam("IsWeekend",
                opt => opt.MapFrom(src => src.IsWeekend == 1))
            .ForCtorParam("PickupCount",
                opt => opt.MapFrom(src => (double)(src.PickupCount ?? 0)))
            .ForCtorParam("DropoffCount",
                opt => opt.MapFrom(src => (double)(src.DropoffCount ?? 0)))
            .ForCtorParam("NetFlow",
                opt => opt.MapFrom(src => (double)(src.NetFlow ?? 0)))
            .ForCtorParam("ActivityRatio",
                opt => opt.MapFrom(src => (double)(src.ActivityRatio ?? 0)))
            .ForCtorParam("Lag1Pickup",
                opt => opt.MapFrom(src => (double)(src.Lag1Pickup ?? 0)))
            .ForCtorParam("Lag1Dropoff",
                opt => opt.MapFrom(src => (double)(src.Lag1Dropoff ?? 0)))
            .ForCtorParam("Lag1NetFlow",
                opt => opt.MapFrom(src => (double)(src.Lag1NetFlow ?? 0)))
            .ForCtorParam("ForecastedDemand6h",
                opt => opt.MapFrom(src => 0.0))
            .ForCtorParam("TempC",
                opt => opt.MapFrom(src => (double)(src.TempC ?? 0)))
            .ForCtorParam("RainMm",
                opt => opt.MapFrom(src => (double)(src.RainMm ?? 0)))
            .ForCtorParam("IsRain",
                opt => opt.MapFrom(src => src.IsRain == 1))
            .ForCtorParam("WeatherCode",
                opt => opt.MapFrom(src => src.WeatherCode ?? 0))
            .ForCtorParam("IsHoliday",
                opt => opt.MapFrom(src => src.IsHoliday == 1));
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

    // =========================================================
    // 7. Simulation
    // =========================================================
    //private void ConfigureSimulationMappings()
    //{
    //    // Simulationrequest → SimulationJobResponse
    //    CreateMap<Simulationrequest, SimulationJobResponse>()
    //        .ForCtorParam("SimulationId",
    //            opt => opt.MapFrom(src => src.SimulationId.ToString()))
    //        .ForCtorParam("Status",
    //            opt => opt.MapFrom(src =>
    //                src.Status == "completed" ? SimulationStatus.Completed :
    //                src.Status == "running" ? SimulationStatus.Running :
    //                src.Status == "failed" ? SimulationStatus.Failed :
    //                SimulationStatus.Pending))
    //        .ForCtorParam("CreatedAt",
    //            opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.UtcNow))
    //        .ForCtorParam("ResultUrl",
    //            opt => opt.MapFrom(src => (string?)null));

    //    // Simulationresult → SimulationMetrics (Baseline)
    //    CreateMap<Simulationresult, SimulationMetrics>()
    //        .ForCtorParam("DemandCoverage",
    //            opt => opt.MapFrom(src =>
    //                src.TargetPickupP50.HasValue && src.DemandForecastP50.HasValue
    //                    ? (double)(src.TargetPickupP50.Value /
    //                      (src.DemandForecastP50.Value == 0 ? 1 : src.DemandForecastP50.Value))
    //                    : 0.0))
    //        .ForCtorParam("AvgWaitTimeMinutes",
    //            opt => opt.MapFrom(src =>
    //                src.EtaP50Sec.HasValue
    //                    ? (double)src.EtaP50Sec.Value / 60.0
    //                    : 0.0))
    //        .ForCtorParam("StockOutRate",
    //            opt => opt.MapFrom(src =>
    //                (double)(src.StockoutProbability ?? 0)))
    //        .ForCtorParam("TotalRevenue",
    //            opt => opt.MapFrom(src =>
    //                (double)(src.ExpectedRevenueP50 ?? 0)))
    //        .ForCtorParam("TotalOperationalCost",
    //            opt => opt.MapFrom(src => 0.0))
    //        .ForCtorParam("NetProfit",
    //            opt => opt.MapFrom(src =>
    //                (double)(src.ExpectedRevenueP50 ?? 0)))
    //        .ForCtorParam("TotalVehicles",
    //            opt => opt.MapFrom(src => 0));

        // Simulationresult → FinancialImpact
        //CreateMap<Simulationresult, FinancialImpact>()
        //    .ForCtorParam("RevenueIncrease",
        //        opt => opt.MapFrom(src =>
        //            (double)((src.ExpectedRevenueP90 ?? 0) -
        //                     (src.ExpectedRevenueP50 ?? 0))))
        //    .ForCtorParam("AdditionalOperationalCost",
        //        opt => opt.MapFrom(src => 0.0))
        //    .ForCtorParam("NetProfitImpact",
        //        opt => opt.MapFrom(src =>
        //            (double)(src.ExpectedRevenueP50 ?? 0)))
        //    .ForCtorParam("PaybackPeriodMonths",
        //        opt => opt.MapFrom(src => (double?)null));
    }
