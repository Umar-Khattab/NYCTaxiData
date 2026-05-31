using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NYCTaxiData.Application.Common.Exceptions;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;
using NYCTaxiData.Domain.EntitiesAi;
using NYCTaxiData.Infrastructure.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Infrastructure.Services;

/// <summary>
/// Infrastructure service that queries <see cref="AiDbContext"/> to load engineered features.
/// Uses No-Tracking queries for optimized, read-only database performance.
/// </summary>
public class AiFeatureProvider : IAiFeatureProvider
{
    private readonly AiDbContext _aiDbContext;
    private readonly ILogger<AiFeatureProvider> _logger;

    // أضف ILogger<AiFeatureProvider> إلى معاملات الـ Constructor
    public AiFeatureProvider(AiDbContext aiDbContext, ILogger<AiFeatureProvider> logger)
    {
        _aiDbContext = aiDbContext;
        _logger = logger; // الآن الـ _logger سيأخذ القيمة القادمة من الـ Constructor
    }

    /// <inheritdoc />
    public async Task<List<Demand15MinInput>> GetDemand15MinFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default)
    {
        var roundedMinute = (targetTime.Minute / 15) * 15;
        var dayOfWeek = (int)targetTime.DayOfWeek;
        var month = targetTime.Month;
        var hour = targetTime.Hour;

        var results = new List<Demand15MinInput>();

        foreach (var zoneId in zoneIds)
        {
            var feature = await _aiDbContext.Demand15mins
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.PuLocationId == zoneId 
                                       && d.Hour == hour 
                                       && d.Minute == roundedMinute 
                                       && d.DayOfWeek == dayOfWeek 
                                       && d.Month == month, ct);

            if (feature is null)
            {
                throw new NotFoundException($"Demand15Min historical feature vector not found in AI database for PU Zone {zoneId} at Rounded Time {hour:D2}:{roundedMinute:D2}.");
            }

            results.Add(new Demand15MinInput(
                feature.PuLocationId ?? zoneId,
                feature.Hour ?? hour,
                feature.Minute ?? roundedMinute,
                feature.DayOfWeek ?? dayOfWeek,
                feature.Month ?? month,
                feature.IsWeekend == 1,
                (double)(feature.Lag1 ?? 0),
                (double)(feature.Lag4 ?? 0),
                (double)(feature.Lag96 ?? 0),
                (double)(feature.RollMean1h ?? 0),
                (double)(feature.RollMean3h ?? 0),
                (double)(feature.TempC ?? 0),
                (double)(feature.RainMm ?? 0),
                feature.IsRain == 1,
                feature.WeatherCode ?? 0,
                feature.PickupCnt ?? 0
            )).ToList();
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is DbUpdateException || ex is InvalidOperationException)
        {
            // تسجيل الخطأ (يفضل استخدام ILogger)
            Console.WriteLine($"[Database Error] Could not fetch AI features: {ex.Message}");

            // إرجاع قائمة فارغة لتجنب الـ Crash
            return new List<Demand15MinInput>();
        }
    }

    /// <inheritdoc />
    public async Task<List<Demand6hInput>> GetDemand6hFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default)
    { 
        var dayOfWeek = (int)targetTime.DayOfWeek;
        var hour = targetTime.Hour;

        var results = new List<Demand6hInput>();

        foreach (var zoneId in zoneIds)
        {
            var feature = await _aiDbContext.Demandfeatures
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.PuLocationId == zoneId 
                                       && d.PickupHour == hour 
                                       && d.DayOfWeek == dayOfWeek, ct);

            if (feature is null)
            {
                throw new NotFoundException($"Demand6h historical feature vector not found in AI database for Zone {zoneId} at Hour {hour}.");
            }

            results.Add(new Demand6hInput(
                feature.PuLocationId ?? zoneId,
                feature.PickupHour ?? hour,
                feature.DayOfWeek ?? dayOfWeek,
                feature.IsWeekend == 1,
                feature.IsHoliday == 1,
                (double)(feature.Lag16h ?? 0),
                (double)(feature.Lag26h ?? 0),
                (double)(feature.Lag46h ?? 0),
                (double)(feature.RollingMean24h ?? 0),
                (double)(feature.TempC ?? 0),
                (double)(feature.RainMm ?? 0),
                feature.IsRain == 1,
                feature.WeatherCode ?? 0,
                feature.PickupCount ?? 0
            ));
        }

        return results;
    }


    /// <inheritdoc />
    public async Task<List<ETAInput>> GetEtaFeaturesAsync(List<RouteRequest> routes, CancellationToken ct = default)
    {
        var results = new List<ETAInput>();

        foreach (var route in routes)
        {
            var pickupHour = route.TargetTime.Hour;
            var pickupDow = (int)route.TargetTime.DayOfWeek;
            var pickupMonth = route.TargetTime.Month;
            var pickupMinute = (route.TargetTime.Minute / 15) * 15;

            var feature = await _aiDbContext.Eta
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.PuLocationId == route.PickupZoneId
                                       && e.DoLocationId == route.DropoffZoneId
                                       && e.PickupHour == pickupHour
                                       && e.PickupDow == pickupDow
                                       && e.PickupMinute == pickupMinute, ct);

            if (feature is null)
            {
                results.Add(new ETAInput(
                    route.PickupZoneId,
                    route.DropoffZoneId,
                    20, 0, 0, 10, pickupHour, pickupDow, pickupMonth, pickupMinute,
                    0, 0, // بدلاً من false، ضع 0 لأن الـ Constructor يتوقع int
                    route.TargetTime, "default", 300, 300, 1, 300
                ));
            }
            else
            {
                // في جزء الـ else:
                results.Add(new ETAInput(
                    feature.PuLocationId ?? 0,
                    feature.DoLocationId ?? 0,
                    feature.TempC ?? 0m,
                    feature.RainMm ?? 0m,
                    feature.WeatherCode ?? 0,
                    (decimal)(feature.DistanceProxy ?? 0),
                    feature.PickupHour ?? 0,
                    feature.PickupDow ?? 0,
                    feature.PickupMonth ?? 0,
                    feature.PickupMinute ?? 0,
                    (feature.IsWeekend == 1 ? 1 : 0), // تحويل من bool إلى int
                    (feature.IsRushHour == 1 ? 1 : 0), // تحويل من bool إلى int
                    route.TargetTime,
                    feature.DistanceBucketLabel ?? "unknown",
                    feature.DurationSec ?? 0m,
                    feature.OdHourMedianDuration ?? 0m,
                    feature.PuHourSlowdownIndex ?? 0m,
                    feature.DistMedianDuration ?? 0 
                ));
            }
        }

        return results;  
    }

    /// <inheritdoc />
    public async Task<List<RevenueInput>> GetRevenueFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default)
    {
        var dayOfWeek = (int)targetTime.DayOfWeek;
        var hour = targetTime.Hour;

        var results = new List<RevenueInput>();

        // 2. معالجة المناطق الموجودة فقط
        foreach (var zoneId in zoneIds)
        {
            var feature = features.FirstOrDefault(r => r.PuLocationId == zoneId);

            if (feature is null)
            {
                throw new NotFoundException($"Revenue historical feature vector not found in AI database for Zone {zoneId} at Hour {hour}.");
            }

            results.Add(new RevenueInput(
    ZoneId: feature.PuLocationId ?? zoneId,
    PickupHour: feature.PickupHour ?? hour,
    DayOfWeek: feature.DayOfWeek ?? dayOfWeek,
    IsWeekend: feature.IsWeekend == 1,
    lag1_6h: (int)(feature.Lag16h ?? 0),
    lag2_6h: (int)(feature.Lag26h ?? 0),
    lag4_6h: (int)(feature.Lag46h ?? 0),
    RevLag1_6h: (double)(feature.RevLag16h ?? 0),
    RevLag1Week: (double)(feature.RevLag1Week ?? 0),
    RevRollingMean7d: (double)(feature.RevRollingMean7d ?? 0),
    RevRollingMean30d: (double)(feature.RevRollingMean30d ?? 0),
    RollingMean24h: (decimal?)feature.RollingMean24h,
    AvgFare: (double)(feature.AvgFare ?? 0),
    TipRate: (double)(feature.TipRate ?? 0),
    TempC: (double?)feature.TempC,
    RainMm: (double?)feature.RainMm,
    IsRain: (feature.IsRain == 1),
    WeatherCode: feature.WeatherCode,
    IsHoliday: (feature.IsHoliday == 1)
));
        }

        return results;
    }
    /// <inheritdoc />
    public async Task<List<StockOutInput>> GetStockOutFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default)
    {
        var dayOfWeek = (int)targetTime.DayOfWeek;
        var hour = targetTime.Hour;

        var results = new List<StockOutInput>();

        foreach (var zoneId in zoneIds)
        {
            var feature = await _aiDbContext.Stockoutfeatures
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ZoneId == zoneId 
                                       && s.Hour == hour 
                                       && s.DayOfWeek == dayOfWeek, ct);

            if (feature is null)
            {
                throw new NotFoundException($"Stockout historical feature vector not found in AI database for Zone {zoneId} at Hour {hour}.");
            }

            results.Add(new StockOutInput(
                feature.ZoneId ?? zoneId,
                feature.TimeBucket6h ?? targetTime,
                (double)(feature.PickupCount ?? 0),
                (double)(feature.DropoffCount ?? 0),
                (double)(feature.NetFlow ?? 0),
                feature.Hour ?? hour,
                feature.DayOfWeek ?? dayOfWeek,
                feature.IsWeekend == 1,
                feature.IsHoliday == 1,
                (double)(feature.ActivityRatio ?? 0),
                (double)(feature.TempC ?? 0),
                (double)(feature.RainMm ?? 0),
                feature.IsRain == 1,
                (double)(feature.Lag1Pickup ?? 0),
                (double)(feature.Lag1Dropoff ?? 0),
                (double)(feature.Lag1NetFlow ?? 0),
                feature.WeatherCode ?? 0
            ));
        }
        return results;
    }
}
