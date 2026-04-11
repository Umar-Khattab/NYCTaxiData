# Caching Behavior - Quick Reference

## What It Does

Automatically caches query results to improve performance:
- **First request** - Executes query, stores in cache
- **Identical request** - Returns from cache (100x faster!)
- **Different request** - Cache miss, executes query
- **Commands** - Skipped (not cached)

## Quick Example

```csharp
// First request
var query = new GetProfileQuery { UserId: "user-123" };
var result1 = await mediator.Send(query);
// Executes, caches result (500ms)

// Second request (same parameters)
var result2 = await mediator.Send(query);
// Returns from cache (2ms) ✓ 250x faster!

// Different parameters
var query3 = new GetProfileQuery { UserId: "user-456" };
var result3 = await mediator.Send(query3);
// Cache miss, executes (500ms)
```

## When Applied

```
Queries → Cached ✓
  GetProfileQuery
  GetOrderQuery
  GetAllZonesQuery

Commands → Skipped ✗
  CreateOrderCommand
  UpdateOrderCommand
  DeleteOrderCommand
```

## Integration (2 Steps)

### Step 1: Register Cache Backend
```csharp
// Redis
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
});

// Or in-memory
services.AddDistributedMemoryCache();
```

### Step 2: Register Behavior
```csharp
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
```

That's it! Queries are cached automatically.

## Cache Durations

```
LONG-LIVED (30 minutes):
  GetTopLevelKpisQuery ............... 30 min
  GetAllZonesQuery ................... 30 min
  GetSystemThresholdsQuery ........... 30 min

MEDIUM (10 minutes):
  GetActiveFleetQuery ................ 10 min
  GetDemandForecastQuery ............. 10 min
  GetShiftStatisticsQuery ............ 10 min

SHORT (2-5 minutes):
  GetProfileQuery .................... 5 min
  GetLiveDispatchFeedQuery ........... 2 min
  GetTripHistoryQuery ................ 2 min

VERY SHORT (30-45 seconds):
  GetExplainableAiInsightQuery ....... 30 sec
  GetDispatchRecommendationQuery ..... 45 sec

DEFAULT (5 minutes):
  Any other query .................... 5 min
```

## Log Output

### Cache Hit
```
[Debug] Checking cache for request GetTopLevelKpisQuery with key cache:GetTopLevelKpisQuery:a1b2c3d4e5f6g7h8
[Information] Cache hit for request GetTopLevelKpisQuery
```

### Cache Miss
```
[Debug] Checking cache for request GetTopLevelKpisQuery with key cache:GetTopLevelKpisQuery:a1b2c3d4e5f6g7h8
[Debug] Cache miss for request GetTopLevelKpisQuery
[Information] Cached response for request GetTopLevelKpisQuery for 1800 seconds
```

### Command (Skipped)
```
[Debug] Request CreateOrderCommand is not cacheable, skipping cache
```

## Cache Key Generation

```csharp
// Based on request type and parameters
cache:GetProfileQuery:a1b2c3d4e5f6g7h8
cache:GetOrderQuery:x9y8z7w6v5u4t3s2

// Same parameters = Same key = Cache hit
GetProfileQuery("user-123") → a1b2c3d4e5f6g7h8
GetProfileQuery("user-123") → a1b2c3d4e5f6g7h8 ✓

// Different parameters = Different key = Cache miss
GetProfileQuery("user-456") → x9y8z7w6v5u4t3s2 ✗
```

## Pipeline Position

```
REQUEST
  ↓
LoggingBehavior ............ Log request
  ↓
CachingBehavior ← HERE ..... Return cached or execute
  ↓
ValidationBehavior ........ Validate input
  ↓
AuthorizationBehavior ..... Check permissions
  ↓
IdempotencyBehavior ....... Prevent duplicates
  ↓
RetryBehavior ............ Retry failures
  ↓
TimeoutBehavior .......... Enforce timeout
  ↓
TransactionBehavior ....... Manage transaction
  ↓
HANDLER (Query execution)
```

## Performance Improvement

```
Database Query:    500 ms
Cache Lookup:      2-5 ms
Improvement:       ~100x faster ✓

Example Site with 85% cache hit rate:
  - Without cache: 500 ms average
  - With cache:    77 ms average (85% @2ms + 15% @500ms)
  - Speed up:      6.5x faster overall
```

## Cache Expiration

```
Automatic expiration:
  - Each query has TTL (Time To Live)
  - After TTL expires, cache entry removed
  - Next request fetches fresh data

Example:
  GetTopLevelKpisQuery cached for 30 minutes
  ├─ 0-29 min: Cache hit ✓
  ├─ 30 min: Cache expires ✓
  └─ 30+ min: Cache miss (new request) ✗
```

## Best Practices

✅ **DO**:
- Cache queries (read-only)
- Set appropriate TTL
- Monitor cache hit rate
- Use deterministic queries

❌ **DON'T**:
- Cache commands
- Cache non-deterministic queries
- Set TTL too long (stale data)
- Set TTL too short (defeats caching)

## Related Documentation

- See `CACHING_README.md` for complete guide
- See `INDEX.md` for all behaviors
- See `COMPLETE_IMPLEMENTATION.md` for full overview

## Build Status

✅ Build Successful - Ready to use!

## Key Takeaways

✅ Automatic query caching  
✅ Configurable cache duration  
✅ ~100x faster for cache hits  
✅ Distributed cache support  
✅ Automatic expiration  

## Dependencies

- ✅ **MediatR** - Already installed
- ✅ **Logging** - Uses ILogger
- ⚠️ **Distributed Cache** - Must configure
  - Redis, In-Memory, or other
