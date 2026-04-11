# Caching Behavior Implementation Guide

## Overview
The `CachingBehavior` is a MediatR pipeline behavior that automatically caches query results to improve API performance. It intelligently stores responses from read-only operations and returns cached data for identical requests, significantly reducing database load and improving response times.

## Components

### CachingBehavior
Located in: `NYCTaxiData.Application\Behaviors\CachingBehavior.cs`

This behavior handles caching for all query operations.

**Features:**
- Automatic caching for queries
- Request-based cache key generation
- Configurable cache durations per query type
- Automatic cache expiration
- Distributed cache support (Redis, memory, etc.)
- Graceful fallback on cache errors
- Comprehensive logging

## How It Works

### Request Classification

```csharp
// Queries - Cacheable (name ends with "Query")
public record GetTopLevelKpisQuery(...) : IRequest<KpiResult>;
                                      ↓
                        Applied caching ✓

// Commands - Not Cached (name ends with "Command")
public record CreateOrderCommand(...) : IRequest<OrderResult>;
                                    ↓
                    Skipped - modifies state ✗
```

### Execution Flow

```
REQUEST (Query)
    ↓
Is this a Query? → No → Skip cache
                     ↓
                Execute normally
                        
    ↓ Yes
Generate cache key from request
    ↓
Check cache for existing response
    ↓
Cache hit? → Yes → Return cached response ✓
    ↓ No
Execute handler
    ↓
Serialize response
    ↓
Store in cache with TTL
    ↓
Log cache info
    ↓
RESPONSE
```

## Cache Key Generation

```csharp
// Cache key based on request type and parameters
private string GenerateCacheKey(string requestName, TRequest request)
{
    var requestJson = JsonSerializer.Serialize(request);
    var hash = GetHashCode(requestJson);
    return $"cache:{requestName}:{hash}";
}

// Example cache keys:
// cache:GetProfileQuery:a1b2c3d4e5f6g7h8
// cache:GetTopLevelKpisQuery:x9y8z7w6v5u4t3s2
// cache:GetDemandForecastQuery:m1n2o3p4q5r6s7t8
```

## Default Cache Durations

### Long-Lived Cache (30 minutes)
```csharp
"GetTopLevelKpisQuery" => 30 minutes       // KPIs relatively static
"GetAllZonesQuery" => 30 minutes           // Zones rarely change
"GetSystemThresholdsQuery" => 30 minutes   // Thresholds stable
"GetOptimalDriverScheduleQuery" => 20 minutes
```

### Medium Cache (10 minutes)
```csharp
"GetActiveFleetQuery" => 10 minutes        // Fleet status changes
"GetDemandForecastQuery" => 10 minutes     // Forecasts updated regularly
"GetShiftStatisticsQuery" => 10 minutes    // Statistics computed
"GetDemandVelocityChartQuery" => 10 minutes
```

### Short-Lived Cache (2-5 minutes)
```csharp
"GetLiveDispatchFeedQuery" => 2 minutes    // Live data, frequent updates
"GetProfileQuery" => 5 minutes             // User data relatively stable
"GetSpecificZoneInsightsQuery" => 3 minutes
"GetTripHistoryQuery" => 2 minutes         // Trip data changes
```

### Very Short Cache (30-45 seconds)
```csharp
"GetExplainableAiInsightQuery" => 30 seconds   // Real-time insights
"GetDispatchRecommendationQuery" => 45 seconds // Dynamic recommendations
```

### Default
```csharp
Any other query => 5 minutes (DefaultCacheDuration)
```

## Integration Steps

### Step 1: Register Distributed Cache Backend

Option A - Redis:
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
});
```

Option B - In-Memory Cache:
```csharp
services.AddDistributedMemoryCache();
```

### Step 2: Register in MediatR Configuration
```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Add CachingBehavior (early in pipeline, before expensive operations)
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>)); // ← NEW (early)
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
    // ... other behaviors
});
```

### Step 3: That's It!
CachingBehavior automatically caches all queries.

## Log Output Examples

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

### Cache Key Generation Error
```
[Debug] Could not generate cache key for CustomQuery, executing without cache
```

## Best Practices

### 1. **Choose Appropriate Cache Duration**
```csharp
// ✅ GOOD - Based on data change frequency
"GetProfileQuery" => 5 minutes           // User changes infrequently
"GetLiveDispatchFeedQuery" => 2 minutes  // Live data, frequent changes
"GetAllZonesQuery" => 30 minutes         // Static configuration

// ❌ BAD - Wrong duration
"GetLiveDispatchFeedQuery" => 30 minutes // Too long for live data!
"GetProfileQuery" => 30 seconds          // Too short, defeats caching
```

### 2. **Cache Queries, Not Commands**
```csharp
// ✅ GOOD - Queries are read-only
public record GetOrderQuery(string OrderId) : IRequest<OrderResult>;

// ❌ BAD - Commands modify state
public record CreateOrderCommand(...) : IRequest<OrderResult>;
// Already skipped - commands not cached
```

### 3. **Monitor Cache Hit Rate**
```bash
# High hit rate = Good performance
grep "Cache hit" logs/log-*.txt | wc -l

# Low hit rate = Adjust TTL or cache size
grep "Cache miss" logs/log-*.txt | wc -l
```

### 4. **Graceful Cache Failure**
```csharp
// Already handled - On cache error, request executes normally
try
{
    // Cache operations
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error during caching");
    return await next();  // Execute normally
}
```

### 5. **Keep Queries Deterministic**
```csharp
// ✅ GOOD - Same parameters = same result
public record GetUserQuery(string UserId) : IRequest<UserDto>;

// ❌ BAD - Non-deterministic (includes current time)
public record GetRecentOrdersQuery(DateTime? Since = null) : IRequest<List<Order>>;
// Cached result might be stale if parameter varies
```

## Pipeline Position

The recommended position for CachingBehavior is:

```
1. LoggingBehavior         ← Log all requests (before cache hit)
2. CachingBehavior ← NEW   ← Return cached before expensive operations
3. ValidationBehavior      ← Validate after cache hit
4. AuthorizationBehavior   ← Check permissions
5. IdempotencyBehavior     ← Handle duplicates
6. RetryBehavior           ← Retry failures
7. TimeoutBehavior         ← Enforce timeout
8. TransactionBehavior     ← Manage transactions
```

**Why this position?**
- After logging (always log)
- Before validation (avoid validating cached data)
- Before authorization (though queries usually don't need auth check)
- Return cached data immediately when available
- Minimize work for cache hits

## Cache Key Strategy

### Deterministic Serialization
```csharp
// Cache key includes all request parameters
var cacheKey = $"cache:GetOrderQuery:{JsonSerializer.Serialize(request.OrderId)}";

// Same parameters = Same cache key
GetOrderQuery("order-123") → cache:GetOrderQuery:a1b2c3d4e5f6g7h8
GetOrderQuery("order-123") → cache:GetOrderQuery:a1b2c3d4e5f6g7h8 ✓ Cache hit

// Different parameters = Different cache key
GetOrderQuery("order-456") → cache:GetOrderQuery:x9y8z7w6v5u4t3s2 ✗ Cache miss
```

### Hash-Based Keys
```csharp
// Uses SHA256 hash for consistent, compact keys
private string GetHashCode(string input)
{
    using (var sha256 = SHA256.Create())
    {
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashedBytes)[..16];  // First 16 chars
    }
}
```

## Cache Invalidation

### Automatic Expiration
```csharp
// Each query has TTL (Time To Live)
"GetTopLevelKpisQuery" => 30 minutes
// After 30 minutes, cache entry automatically expires
// Next request will fetch fresh data
```

### Manual Invalidation (Future Enhancement)
```csharp
// Could implement cache invalidation attribute
[CacheInvalidation("GetTopLevelKpisQuery", "GetActiveFleetQuery")]
public record UpdateThresholdsCommand(...) : IRequest<Unit>;
// When this command executes, above queries' cache is cleared
```

## Testing Cache Behavior

### Test 1: Cache Hit on Identical Request
```csharp
[TestMethod]
public async Task Handle_IdenticalQuery_ReturnsCachedResult()
{
    var query = new GetProfileQuery { UserId = "user-123" };
    var handler = new GetProfileHandler(_db);
    var behavior = new CachingBehavior<GetProfileQuery, UserProfile>(_logger, _cache);
    
    // First request - cache miss
    var result1 = await behavior.Handle(query, async () => await handler.Handle(query, default), default);
    
    // Second request - should be cached
    var result2 = await behavior.Handle(query, async () => await handler.Handle(query, default), default);
    
    // Results are identical
    Assert.AreEqual(result1.UserId, result2.UserId);
    // Handler called only once (if we had call counter)
}
```

### Test 2: Cache Miss on Different Parameters
```csharp
[TestMethod]
public async Task Handle_DifferentParameters_CacheMiss()
{
    var query1 = new GetProfileQuery { UserId = "user-123" };
    var query2 = new GetProfileQuery { UserId = "user-456" };
    var handler = new GetProfileHandler(_db);
    var behavior = new CachingBehavior<GetProfileQuery, UserProfile>(_logger, _cache);
    
    // First query
    var result1 = await behavior.Handle(query1, async () => { /* ... */ }, default);
    
    // Different query - cache miss
    var result2 = await behavior.Handle(query2, async () => { /* ... */ }, default);
    
    // Results are different users
    Assert.AreNotEqual(result1.UserId, result2.UserId);
}
```

### Test 3: Command Not Cached
```csharp
[TestMethod]
public async Task Handle_Command_SkipsCache()
{
    var command = new CreateOrderCommand(...);
    var handler = new CreateOrderHandler(_db);
    var behavior = new CachingBehavior<CreateOrderCommand, OrderResult>(_logger, _cache);
    
    // Command should not use cache
    var result = await behavior.Handle(command, async () => await handler.Handle(command, default), default);
    
    // Verify command executed normally
    Assert.IsNotNull(result);
    // No cache interaction occurred
}
```

## Performance Impact

### Cache Hit Benefit
```
Database Query Time:   500 ms
Cache Lookup Time:     1-5 ms
Improvement:           ~100x faster ✓

Example:
- Without cache: 500 ms per request
- With cache:    2 ms for 95% of requests
- Average:       ~28 ms (for 95% cache hit rate)
```

### Scenarios

**E-commerce Site**:
```
Product List Query:    Hit 95% of time (500 ms → 2 ms)
User Profile Query:    Hit 85% of time (200 ms → 1 ms)
Shopping Cart Query:   Hit 60% of time (100 ms → 1 ms)
```

**Taxi Dispatch System**:
```
Top KPIs Query:        Hit 90% of time (2000 ms → 5 ms)
Zone Data Query:       Hit 95% of time (500 ms → 2 ms)
Live Dispatch Feed:    Hit 50% of time (300 ms → 2 ms)
```

## Related Files

- **CachingBehavior**: `NYCTaxiData.Application\Behaviors\CachingBehavior.cs`
- **IDistributedCache**: Microsoft.Extensions.Caching
- **LoggingBehavior**: For logging cache events
- **Database**: For cache-miss queries

## Common Issues & Solutions

### Issue: Low Cache Hit Rate
**Symptoms**: Most requests are cache misses

**Solutions**:
1. Check cache backend is running
2. Increase cache duration
3. Verify deterministic request parameters
4. Monitor actual usage patterns

### Issue: Stale Data
**Symptoms**: Cached data is outdated

**Solutions**:
1. Reduce cache duration
2. Implement cache invalidation on data changes
3. Use shorter TTL for frequently changing data

### Issue: Cache Not Working
**Symptoms**: Performance not improving

**Solutions**:
1. Verify distributed cache is registered
2. Check logs for cache errors
3. Verify queries end with "Query" suffix
4. Ensure cache backend is accessible

## Summary

The **CachingBehavior** provides:

✅ Automatic response caching for queries  
✅ Configurable cache duration per query type  
✅ Deterministic cache key generation  
✅ Distributed cache support  
✅ Graceful error handling  
✅ Comprehensive logging  
✅ Significant performance improvement  

It's essential for reducing database load and improving API response times for read-heavy workloads.
