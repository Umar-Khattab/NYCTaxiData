using AutoMapper;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Features.AI.DTOs;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Infrastructure;

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
        // Entity -> DTO mappings
        CreateMap<Simulationrequest, SimulationJobResponse>();
        CreateMap<SimulationResult, SimulationResult>();

        // Nested mappings
        CreateMap<SimulationResult, SimulationMetrics>();

        // Demand prediction mappings
        CreateMap<Demandprediction, Demand15MinResult>()
            .ForMember(dest => dest.ZoneId, opt => opt.MapFrom(src => src.ZoneId))
            .ForMember(dest => dest.P50, opt => opt.MapFrom(src => src.P50))
            .ForMember(dest => dest.P90, opt => opt.MapFrom(src => src.P90))
            .ForMember(dest => dest.LowerBound, opt => opt.Ignore())
            .ForMember(dest => dest.UpperBound, opt => opt.Ignore());

        // Zone supply state mappings
        CreateMap<Zone, ZoneSupplyState>()
            .ForMember(dest => dest.CurrentSupply, opt => opt.Ignore())
            .ForMember(dest => dest.ActiveTrips, opt => opt.Ignore())
            .ForMember(dest => dest.ForecastedDemand, opt => opt.Ignore())
            .ForMember(dest => dest.StockOutRisk, opt => opt.Ignore())
            .ForMember(dest => dest.ExpectedRevenue, opt => opt.Ignore());

        // Simulation result nested mappings
        CreateMap<Simulationrequest, SimulationResult>()
            .ForMember(dest => dest.SimulationId, opt => opt.MapFrom(src => src.SimulationId))
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.BaselineMetrics, opt => opt.Ignore())
            .ForMember(dest => dest.SimulatedMetrics, opt => opt.Ignore())
            .ForMember(dest => dest.FinancialImpact, opt => opt.Ignore())
            .ForMember(dest => dest.ZoneBreakdown, opt => opt.Ignore())
            .ForMember(dest => dest.Recommendation, opt => opt.Ignore());
    }
}
