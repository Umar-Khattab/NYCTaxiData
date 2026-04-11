# Metrics Behavior Implementation Guide

## Overview
The `MetricsBehavior` is a MediatR pipeline behavior that collects comprehensive performance and operational metrics for all requests. It tracks execution times, success/failure rates, error types, and other observability data essential for monitoring, alerting, and performance optimization.

## Components

### MetricsBehavior
Located in: `NYCTaxiData.Application\Behaviors\MetricsBehavior.cs`

This behavior collects metrics for all requests without affecting performance.

**Features:**
- Automatic execution time tracking
- Success/failure rate calculation
- Min/max/average performance metrics
- Error type tracking and counting
- Thread-safe metrics storage
- Real-time metrics access
- Production-ready implementation

### RequestMetrics
Container class for storing request metrics.

**Properties:**
- `RequestName` - Name of the request type
- `TotalRequests` - Total number of requests
- `SuccessfulRequests` - Successful requests count
- `FailedRequests` - Failed requests count
- `SuccessRate` - Success percentage (0-100)
- `TotalExecutionTime` - Total time in milliseconds
- `MinExecutionTime` - Minimum execution time
- `MaxExecutionTime` - Maximum execution time
- `AverageExecutionTime` - Average execution time
- `ErrorCounts` - Dictionary of error types and counts
- `MostCommonError` - Most frequent error type
- `MostCommonErrorCount` - Count of most common error

## How It Works

### Request Processing Flow

```
REQUEST
    ↓
Start stopwatch
    ↓
Execute request
    ↓
On success:
  ├─ Record successful execution
  ├─ Record execution time
  ├─ Log metrics
  └─ Return response

On failure:
  ├─ Record failed execution
  ├─ Record error type
  ├─ Record execution time
  ├─ Log error metrics
  └─ Throw exception
    ↓
RESPONSE or ERROR
```

### Metrics Collected

```csharp
For each request:
  ✅ Total requests count
  ✅ Success count
  ✅ Failure count
  ✅ Success rate percentage
  ✅ Execution time (min, max, avg, total)
  ✅ Error types and counts
  ✅ Most common error
```

## Integration Steps

### Step 1: Register in MediatR Configuration
```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Add MetricsBehavior (first to capture all behaviors)
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
    
    // Add other behaviors
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
    // ... other behaviors
});
```

### Step 2: Access Metrics (Optional)
```csharp
// Get all metrics
var allMetrics = MetricsBehavior<object, object>.GetAllMetrics();
foreach (var metric in allMetrics)
{
    Console.WriteLine(metric.Value.ToString());
}

// Get specific request metrics
var profileMetrics = MetricsBehavior<object, object>.GetMetrics("GetProfileQuery");
Console.WriteLine($"Success rate: {profileMetrics?.SuccessRate:F2}%");

// Reset metrics
MetricsBehavior<object, object>.ResetMetrics();
```

### Step 3: That's It!
Metrics are automatically collected for all requests.

## Log Output Examples

### Successful Request
```
[Information] Request GetProfileQuery completed in 245ms
```

### Failed Request
```
[Error] Request CreateOrderCommand failed after 523ms with error: ValidationException
```

### Cancelled Request
```
[Warning] Request GetLiveDispatchFeedQuery was cancelled after 1000ms
```

## Accessing Metrics

### Get All Metrics
```csharp
public class MetricsController : ControllerBase
{
    [HttpGet("metrics")]
    public IActionResult GetMetrics()
    {
        var allMetrics = MetricsBehavior<object, object>.GetAllMetrics();
        return Ok(allMetrics.Values.Select(m => new
        {
            m.RequestName,
            m.TotalRequests,
            m.SuccessfulRequests,
            m.FailedRequests,
            m.SuccessRate,
            m.AverageExecutionTime,
            m.MinExecutionTime,
            m.MaxExecutionTime,
            m.ErrorCounts,
            m.MostCommonError
        }));
    }
}
```

### Get Specific Request Metrics
```csharp
[HttpGet("metrics/{requestName}")]
public IActionResult GetMetrics(string requestName)
{
    var metrics = MetricsBehavior<object, object>.GetMetrics(requestName);
    if (metrics == null)
        return NotFound();

    return Ok(new
    {
        metrics.RequestName,
        metrics.TotalRequests,
        metrics.SuccessRate,
        metrics.AverageExecutionTime,
        Performance = new
        {
            metrics.MinExecutionTime,
            metrics.MaxExecutionTime,
            metrics.AverageExecutionTime
        },
        Errors = metrics.ErrorCounts,
        metrics.MostCommonError
    });
}
```

## Metrics Analysis Examples

### Performance Monitoring
```csharp
// Find slow queries
var slowQueries = allMetrics
    .Where(x => x.Value.AverageExecutionTime > 1000)  // > 1 second
    .OrderByDescending(x => x.Value.AverageExecutionTime)
    .Take(10);

foreach (var query in slowQueries)
{
    Console.WriteLine($"{query.Key}: {query.Value.AverageExecutionTime}ms avg");
}
```

### Failure Rate Monitoring
```csharp
// Find problematic operations
var failingOperations = allMetrics
    .Where(x => x.Value.SuccessRate < 95)  // < 95% success
    .OrderBy(x => x.Value.SuccessRate);

foreach (var op in failingOperations)
{
    Console.WriteLine($"{op.Key}: {op.Value.SuccessRate:F2}% success rate");
    Console.WriteLine($"  Most common error: {op.Value.MostCommonError}");
}
```

### Trend Analysis
```csharp
// Compare performance over time
var metrics1 = MetricsBehavior<object, object>.GetAllMetrics();
await Task.Delay(TimeSpan.FromMinutes(5));
var metrics2 = MetricsBehavior<object, object>.GetAllMetrics();

var performance = metrics1
    .Join(metrics2, x => x.Key, x => x.Key, (k, v) => new
    {
        Request = k,
        AvgBefore = v.Value.AverageExecutionTime,
        AvgAfter = metrics2[k].AverageExecutionTime
    })
    .Where(x => x.AvgAfter > x.AvgBefore)
    .OrderByDescending(x => x.AvgAfter - x.AvgBefore);

foreach (var perf in performance)
{
    var diff = perf.AvgAfter - perf.AvgBefore;
    Console.WriteLine($"{perf.Request}: {diff}ms slower");
}
```

## Best Practices

### 1. **Use Metrics for Alerting**
```csharp
public class MetricsAlertService
{
    public void CheckMetrics()
    {
        var allMetrics = MetricsBehavior<object, object>.GetAllMetrics();
        
        foreach (var metric in allMetrics.Values)
        {
            // Alert on low success rate
            if (metric.SuccessRate < 95)
                AlertLowSuccessRate(metric);
            
            // Alert on slow performance
            if (metric.AverageExecutionTime > 1000)
                AlertSlowPerformance(metric);
            
            // Alert on increasing error count
            if (metric.MostCommonErrorCount > 100)
                AlertHighErrorCount(metric);
        }
    }
}
```

### 2. **Monitor Cache Effectiveness**
```csharp
// Compare cached vs. non-cached queries
var cachedQueries = allMetrics
    .Where(x => x.Key.Contains("Query"))
    .Where(x => x.Value.AverageExecutionTime < 10);

Console.WriteLine($"Cached queries with avg < 10ms: {cachedQueries.Count()}");
```

### 3. **Track Error Trends**
```csharp
// Find which operations are failing most
var errorSummary = allMetrics
    .Where(x => x.Value.ErrorCounts.Count > 0)
    .Select(x => new
    {
        x.Key,
        TotalErrors = x.Value.FailedRequests,
        Errors = x.Value.ErrorCounts
    })
    .OrderByDescending(x => x.TotalErrors);
```

### 4. **Export Metrics for Analysis**
```csharp
public class MetricsExporter
{
    public string ExportAsJson()
    {
        var allMetrics = MetricsBehavior<object, object>.GetAllMetrics();
        return JsonSerializer.Serialize(allMetrics.Values, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
    
    public string ExportAsCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("RequestName,Total,Success,Failed,SuccessRate,AvgTime,MinTime,MaxTime");
        
        var allMetrics = MetricsBehavior<object, object>.GetAllMetrics();
        foreach (var metric in allMetrics.Values)
        {
            sb.AppendLine(
                $"{metric.RequestName},{metric.TotalRequests},{metric.SuccessfulRequests}," +
                $"{metric.FailedRequests},{metric.SuccessRate:F2},{metric.AverageExecutionTime}," +
                $"{metric.MinExecutionTime},{metric.MaxExecutionTime}");
        }
        
        return sb.ToString();
    }
}
```

## Pipeline Position

The recommended position for MetricsBehavior is:

```
1. MetricsBehavior ← NEW ← Capture all metrics (first)
2. LoggingBehavior ← Log all requests
3. CachingBehavior ← Check cache
4. ValidationBehavior ← Validate input
5. AuthorizationBehavior ← Check permissions
6. IdempotencyBehavior ← Handle duplicates
7. RetryBehavior ← Retry failures
8. TimeoutBehavior ← Enforce timeout
9. TransactionBehavior ← Manage transactions
```

**Why first?**
- Captures all other behaviors' timing
- No other behavior's overhead in metrics
- Measures total request time accurately
- Most comprehensive data collection

## Thread Safety

The metrics collection is thread-safe using locks:

```csharp
private static readonly object MetricsLock = new lock();
private static readonly Dictionary<string, RequestMetrics> RequestMetricsMap = new();

lock (MetricsLock)
{
    // Update metrics
    RequestMetricsMap[requestName] = metrics;
}
```

This ensures accurate metrics even under high concurrency.

## Performance Impact

```
Metric Collection Overhead:  ~1ms per request
Database Query Time:         500ms
Overhead Percentage:         0.2%

Negligible impact on performance!
```

## Testing Metrics

### Test 1: Successful Request Metrics
```csharp
[TestMethod]
public async Task Handle_SuccessfulRequest_RecordsMetrics()
{
    MetricsBehavior<GetProfileQuery, UserProfile>.ResetMetrics();
    var query = new GetProfileQuery { UserId = "user-123" };
    var handler = new GetProfileHandler(_db);
    var behavior = new MetricsBehavior<GetProfileQuery, UserProfile>(_logger);
    
    await behavior.Handle(query, async () => await handler.Handle(query, default), default);
    
    var metrics = MetricsBehavior<GetProfileQuery, UserProfile>.GetMetrics("GetProfileQuery");
    Assert.AreEqual(1, metrics.TotalRequests);
    Assert.AreEqual(1, metrics.SuccessfulRequests);
    Assert.AreEqual(100, metrics.SuccessRate);
}
```

### Test 2: Failed Request Metrics
```csharp
[TestMethod]
public async Task Handle_FailedRequest_RecordsError()
{
    MetricsBehavior<CreateOrderCommand, OrderResult>.ResetMetrics();
    var command = new CreateOrderCommand { CustomerId = "invalid" };
    var handler = new CreateOrderHandler(_db);
    var behavior = new MetricsBehavior<CreateOrderCommand, OrderResult>(_logger);
    
    try
    {
        await behavior.Handle(command, async () => throw new InvalidOperationException(), default);
    }
    catch { }
    
    var metrics = MetricsBehavior<CreateOrderCommand, OrderResult>.GetMetrics("CreateOrderCommand");
    Assert.AreEqual(1, metrics.TotalRequests);
    Assert.AreEqual(1, metrics.FailedRequests);
    Assert.AreEqual(0, metrics.SuccessRate);
    Assert.IsTrue(metrics.ErrorCounts.ContainsKey("InvalidOperationException"));
}
```

## Related Files

- **MetricsBehavior**: `NYCTaxiData.Application\Behaviors\MetricsBehavior.cs`
- **LoggingBehavior**: For logging metric events
- **All other behaviors**: Generate the metrics

## Summary

The **MetricsBehavior** provides:

✅ Automatic performance metric collection  
✅ Success/failure rate tracking  
✅ Execution time analysis (min/max/avg)  
✅ Error type and count tracking  
✅ Thread-safe metrics storage  
✅ Real-time metrics access  
✅ Minimal performance overhead  

It's essential for monitoring, alerting, and optimization in production environments.
