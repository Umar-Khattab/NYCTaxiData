//using AutoMapper;
//using NYCTaxiData.Application.Features.Analytics.Commands.UpdateSystemThresholds;
//using NYCTaxiData.Application.Features.Analytics.Queries.GetSystemThresholds;

//namespace NYCTaxiData.Application.Features.Analytics.Mappings
//{
//    public class AnalyticsMappingProfile : Profile
//    {
//        public AnalyticsMappingProfile()
//        {
//            /*
//             * NOTE: KPIs and Chart DTOs (e.g., GetTopLevelKpisQuery and GetDemandVelocityChartQuery) 
//             * are mapped manually via EF Core projections in their respective Handlers for 
//             * performance optimization. Therefore, they do not need AutoMapper configurations here.
//             */

//            // System Thresholds Mappings
//            CreateMap<NYCTaxiData.Domain.Entities.SystemThreshold, SystemThresholdsDto>();
//            CreateMap<UpdateSystemThresholdsCommand, NYCTaxiData.Domain.Entities.SystemThreshold>();
//        }
//    }
//}
