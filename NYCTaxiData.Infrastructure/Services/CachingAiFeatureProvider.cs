using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.AI;

namespace NYCTaxiData.Infrastructure.Services;

/// <summary>
/// Thread-safe Caching Decorator that wraps <see cref="IAiFeatureProvider"/>.
/// Caches zone-specific historical features in-memory with a 6-hour sliding expiration,
/// performing database fetches ONLY for missing cache records.
/// </summary>
public class CachingAiFeatureProvider : IAiFeatureProvider
{
    private readonly IAiFeatureProvider _innerProvider;
    private readonly IMemoryCache _memoryCache;

    /// <summary>Sliding expiration — resets on each access within the window.</summary>
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(6);

    /// <summary>Absolute cap — cache entry is always evicted after this period regardless of traffic.</summary>
    private static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromHours(24);

    private MemoryCacheEntryOptions CacheOptions => new MemoryCacheEntryOptions()
        .SetSlidingExpiration(SlidingExpiration)
        .SetAbsoluteExpiration(AbsoluteExpiration);

    public CachingAiFeatureProvider(IAiFeatureProvider innerProvider, IMemoryCache memoryCache)
    {
        _innerProvider = innerProvider;
        _memoryCache = memoryCache;
    }

    /// <inheritdoc />
    public async Task<List<Demand15MinInput>> GetDemand15MinFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default)
    {
        var roundedMinute = (targetTime.Minute / 15) * 15;
        var timeKey = new DateTime(targetTime.Year, targetTime.Month, targetTime.Day, targetTime.Hour, roundedMinute, 0);

        var results = new List<Demand15MinInput>();
        var missingZones = new List<int>();

        foreach (var zoneId in zoneIds)
        {
            var cacheKey = $"feat:demand15m:{zoneId}:{timeKey:yyyyMMddHHmm}";
            if (_memoryCache.TryGetValue<Demand15MinInput>(cacheKey, out var cached))
            {
                results.Add(cached!);
            }
            else
            {
                missingZones.Add(zoneId);
            }
        }

        if (missingZones.Count > 0)
        {
            var loaded = await _innerProvider.GetDemand15MinFeaturesAsync(missingZones, targetTime, ct);
            foreach (var item in loaded)
            {
                var cacheKey = $"feat:demand15m:{item.ZoneId}:{timeKey:yyyyMMddHHmm}";
                _memoryCache.Set(cacheKey, item, CacheOptions);
                results.Add(item);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<List<Demand6hInput>> GetDemand6hFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default)
    {
        var timeKey = new DateTime(targetTime.Year, targetTime.Month, targetTime.Day, targetTime.Hour, 0, 0);

        var results = new List<Demand6hInput>();
        var missingZones = new List<int>();

        foreach (var zoneId in zoneIds)
        {
            var cacheKey = $"feat:demand6h:{zoneId}:{timeKey:yyyyMMddHH}";
            if (_memoryCache.TryGetValue<Demand6hInput>(cacheKey, out var cached))
            {
                results.Add(cached!);
            }
            else
            {
                missingZones.Add(zoneId);
            }
        }

        if (missingZones.Count > 0)
        {
            var loaded = await _innerProvider.GetDemand6hFeaturesAsync(missingZones, targetTime, ct);
            foreach (var item in loaded)
            {
                var cacheKey = $"feat:demand6h:{item.ZoneId}:{timeKey:yyyyMMddHH}";
                _memoryCache.Set(cacheKey, item, CacheOptions);
                results.Add(item);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<List<ETAInput>> GetEtaFeaturesAsync(List<RouteRequest> routes, CancellationToken ct = default)
    {
        var results = new List<ETAInput>();
        var missingRoutes = new List<RouteRequest>();

        foreach (var route in routes)
        {
            var roundedMinute = (route.TargetTime.Minute / 15) * 15;
            var timeKey = new DateTime(route.TargetTime.Year, route.TargetTime.Month, route.TargetTime.Day, route.TargetTime.Hour, roundedMinute, 0);
            var cacheKey = $"feat:eta:{route.PickupZoneId}:{route.DropoffZoneId}:{timeKey:yyyyMMddHHmm}";

            if (_memoryCache.TryGetValue<ETAInput>(cacheKey, out var cached))
            {
                results.Add(cached!);
            }
            else
            {
                missingRoutes.Add(route);
            }
        }

        if (missingRoutes.Count > 0)
        {
            var loaded = await _innerProvider.GetEtaFeaturesAsync(missingRoutes, ct);
            foreach (var item in loaded)
            {
                var route = missingRoutes.FirstOrDefault(r => r.PickupZoneId == item.PickupZoneId && r.DropoffZoneId == item.DropoffZoneId);
                if (route != null)
                {
                    var roundedMinute = (route.TargetTime.Minute / 15) * 15;
                    var timeKey = new DateTime(route.TargetTime.Year, route.TargetTime.Month, route.TargetTime.Day, route.TargetTime.Hour, roundedMinute, 0);
                    var cacheKey = $"feat:eta:{item.PickupZoneId}:{item.DropoffZoneId}:{timeKey:yyyyMMddHHmm}";

                    _memoryCache.Set(cacheKey, item, CacheOptions);
                }
                results.Add(item);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<List<RevenueInput>> GetRevenueFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default)
    {
        var timeKey = new DateTime(targetTime.Year, targetTime.Month, targetTime.Day, targetTime.Hour, 0, 0);

        var results = new List<RevenueInput>();
        var missingZones = new List<int>();

        foreach (var zoneId in zoneIds)
        {
            var cacheKey = $"feat:rev:{zoneId}:{timeKey:yyyyMMddHH}";
            if (_memoryCache.TryGetValue<RevenueInput>(cacheKey, out var cached))
            {
                results.Add(cached!);
            }
            else
            {
                missingZones.Add(zoneId);
            }
        }

        if (missingZones.Count > 0)
        {
            var loaded = await _innerProvider.GetRevenueFeaturesAsync(missingZones, targetTime, ct);
            foreach (var item in loaded)
            {
                var cacheKey = $"feat:rev:{item.ZoneId}:{timeKey:yyyyMMddHH}";
                _memoryCache.Set(cacheKey, item, CacheOptions);
                results.Add(item);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<List<StockOutInput>> GetStockOutFeaturesAsync(List<int> zoneIds, DateTime targetTime, CancellationToken ct = default)
    {
        var timeKey = new DateTime(targetTime.Year, targetTime.Month, targetTime.Day, targetTime.Hour, 0, 0);

        var results = new List<StockOutInput>();
        var missingZones = new List<int>();

        foreach (var zoneId in zoneIds)
        {
            var cacheKey = $"feat:stock:{zoneId}:{timeKey:yyyyMMddHH}";
            if (_memoryCache.TryGetValue<StockOutInput>(cacheKey, out var cached))
            {
                results.Add(cached!);
            }
            else
            {
                missingZones.Add(zoneId);
            }
        }

        if (missingZones.Count > 0)
        {
            var loaded = await _innerProvider.GetStockOutFeaturesAsync(missingZones, targetTime, ct);
            foreach (var item in loaded)
            {
                var cacheKey = $"feat:stock:{item.ZoneId}:{timeKey:yyyyMMddHH}";
                _memoryCache.Set(cacheKey, item, CacheOptions);
                results.Add(item);
            }
        }

        return results;
    }
}
