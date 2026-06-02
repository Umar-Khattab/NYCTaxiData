using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.Common.Interfaces.Simulation;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetHighStockoutZones
{
    public class GetHighStockoutZonesQueryHandler : IRequestHandler<GetHighStockoutZonesQuery, Result<List<HighStockoutZoneDto>>>
    {
        private readonly IMemoryCache _cache;
        private readonly ISimulationOrchestrator _orchestrator;
        private readonly IAiPredictionService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public GetHighStockoutZonesQueryHandler(
            IMemoryCache cache,
            ISimulationOrchestrator orchestrator,
            IAiPredictionService aiService,
            IUnitOfWork unitOfWork)
        {
            _cache = cache;
            _orchestrator = orchestrator;
            _aiService = aiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<HighStockoutZoneDto>>> Handle(
            GetHighStockoutZonesQuery request,
            CancellationToken cancellationToken)
        {
            var limit = request.Limit > 0 ? request.Limit : 10;

            // 1. Simulation Mode Interceptor
            var simStatus = _orchestrator.GetStatus();
            if (simStatus.Status == SimulationStatus.Running)
            {
                var latestTick = _orchestrator.GetLatestTick();
                if (latestTick != null && latestTick.Zones.Count > 0)
                {
                    var sortedSimZones = latestTick.Zones
                        .OrderByDescending(z => z.StockoutRisk)
                        .Take(limit)
                        .ToList();

                    var zonesList = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);
                    var zoneDict = zonesList.ToDictionary(z => z.ZoneId, z => z);
                    var simResult = new List<HighStockoutZoneDto>();

                    foreach (var zone in sortedSimZones)
                    {
                        var zoneName = "Simulated Zone " + zone.ZoneId;
                        var borough = "Manhattan";

                        if (zoneDict.TryGetValue(zone.ZoneId, out var dbZone))
                        {
                            zoneName = dbZone.ZoneName;
                            borough = dbZone.Borough ?? "Unknown";
                        }

                        int deficit = Math.Max(0, (int)zone.Demand - zone.DriverCount);

                        simResult.Add(new HighStockoutZoneDto
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zoneName,
                            Borough = borough,
                            CalculatedDeficit = deficit,
                            CalculatedStockoutProbability = Math.Round(zone.StockoutRisk, 4),
                            PredictedDeficit = deficit,
                            PredictedStockoutProbability = Math.Round(zone.StockoutRisk * 1.15, 4),

                            // Legacy Support
                            PickupCount = (int)zone.Demand,
                            AvailableDriversCount = zone.DriverCount,
                            DeficitCount = deficit,
                            StockoutProbability = Math.Round(zone.StockoutRisk, 4)
                        });
                    }

                    return Result<List<HighStockoutZoneDto>>.Success(simResult, "High stockout risk zones resolved from simulated state");
                }
            }

            // 2. Short-Term Memory Cache for High Performance
            var cacheKey = $"HighStockoutZones_L_{limit}";
            if (_cache.TryGetValue(cacheKey, out List<HighStockoutZoneDto>? cachedData) && cachedData != null)
            {
                return Result<List<HighStockoutZoneDto>>.Success(cachedData, "High stockout risk zones retrieved from cache");
            }

            // 3. High-Speed Parallel Execution
            // 3. High-Speed Sequential Execution to ensure DbContext thread safety
            var demandList = await _unitOfWork.Trips.Query()
                .AsNoTracking()
                .Where(t => t.PickupLocation != null && t.PickupLocation.ZoneId != null)
                .GroupBy(t => t.PickupLocation!.ZoneId!.Value)
                .Select(g => new { ZoneId = g.Key, PickupCount = g.Count() })
                .ToListAsync(cancellationToken);

            var activeDriversInfo = await _unitOfWork.Drivers.Query()
                .AsNoTracking()
                .Where(d => d.Status == CurrentStatus.Available)
                .Select(d => new
                {
                    DriverId = d.UserId,
                    LatestLocationId = d.Trips
                        .OrderByDescending(t => t.StartedAt)
                        .Select(t => t.DropoffLocationId)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var demandDict = demandList.ToDictionary(d => d.ZoneId, d => d.PickupCount);
            
            var locations = await _unitOfWork.Locations.Query().AsNoTracking().ToListAsync(cancellationToken);
            var locDict = locations.ToDictionary(l => l.LocationId, l => l);
            var zones = await _unitOfWork.Zones.Query().AsNoTracking().ToListAsync(cancellationToken);

            var driverSupplyDict = zones.ToDictionary(z => z.ZoneId, z => 0);

            foreach (var driver in activeDriversInfo)
            {
                if (driver.LatestLocationId.HasValue && locDict.TryGetValue(driver.LatestLocationId.Value, out var loc) && loc.ZoneId.HasValue)
                {
                    if (driverSupplyDict.ContainsKey(loc.ZoneId.Value))
                    {
                        driverSupplyDict[loc.ZoneId.Value]++;
                    }
                }
            }

            var stockoutList = new List<HighStockoutZoneDto>();

            // 4. FastAPI AI batch predictions
            var batchStockInputs = zones.Select(z => {
                int pickups = demandDict.GetValueOrDefault(z.ZoneId, 0);
                int drivers = driverSupplyDict.GetValueOrDefault(z.ZoneId, 0);
                int deficit = Math.Max(0, pickups - drivers);
                
                return new StockOutInput(
                    z.ZoneId, DateTime.UtcNow, pickups, drivers, deficit,
                    DateTime.UtcNow.Hour, (int)DateTime.UtcNow.DayOfWeek,
                    DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday,
                    false, 1.0, 20.0, 0.0, false, 0, 0, 0, 0
                );
            }).ToList();

            List<StockOutResult> predictions = new();
            try
            {
                predictions = await _aiService.PredictStockOutAsync(batchStockInputs, cancellationToken);
            }
            catch (Exception)
            {
                predictions = zones.Select(z => {
                    int pickups = demandDict.GetValueOrDefault(z.ZoneId, 0);
                    int drivers = driverSupplyDict.GetValueOrDefault(z.ZoneId, 0);
                    double prob = pickups > 0 ? (double)pickups / (pickups + drivers + 1) : 0.0;
                    return new StockOutResult(z.ZoneId, prob * 1.1);
                }).ToList();
            }

            var predDict = predictions.ToDictionary(p => p.ZoneId, p => p.Probability);

            foreach (var zone in zones)
            {
                int pickups = demandDict.GetValueOrDefault(zone.ZoneId, 0);
                int availableDrivers = driverSupplyDict.GetValueOrDefault(zone.ZoneId, 0);

                int calcDeficit = Math.Max(0, pickups - availableDrivers);
                double calcProb = pickups > 0 ? (double)pickups / (pickups + availableDrivers + 1) : 0.0;

                double predProb = predDict.GetValueOrDefault(zone.ZoneId, calcProb * 1.1);
                int predDeficit = Math.Max(0, (int)(pickups * 1.1) - availableDrivers);

                stockoutList.Add(new HighStockoutZoneDto
                {
                    ZoneId = zone.ZoneId,
                    ZoneName = zone.ZoneName,
                    Borough = zone.Borough ?? "Unknown",
                    CalculatedDeficit = calcDeficit,
                    CalculatedStockoutProbability = Math.Round(calcProb, 4),
                    PredictedDeficit = predDeficit,
                    PredictedStockoutProbability = Math.Round(predProb, 4),

                    // Legacy Support
                    PickupCount = pickups,
                    AvailableDriversCount = availableDrivers,
                    DeficitCount = calcDeficit,
                    StockoutProbability = Math.Round(calcProb, 4)
                });
            }

            var topStockouts = stockoutList
                .OrderByDescending(x => x.PredictedStockoutProbability)
                .ThenByDescending(x => x.CalculatedDeficit)
                .Take(limit)
                .ToList();

            _cache.Set(cacheKey, topStockouts, TimeSpan.FromSeconds(15));

            return Result<List<HighStockoutZoneDto>>.Success(topStockouts, "High stockout risk zones identified successfully");
        }
    }
}
