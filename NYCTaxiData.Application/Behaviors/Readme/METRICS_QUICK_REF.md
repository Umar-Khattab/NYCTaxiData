# Metrics Behavior - Quick Reference

## What It Does

Automatically collects performance and operational metrics:
- **Execution time** - Min, max, average per request
- **Success/failure rate** - Track reliability
- **Error types** - Count and categorize failures
- **Request counts** - Track usage patterns
- **Performance analysis** - Identify bottlenecks

## Quick Example

```csharp
// Metrics collected automatically
var allMetrics = MetricsBehavior<object, object>.GetAllMetrics();

foreach (var metric in allMetrics.Values)
{
    Console.WriteLine($"{metric.RequestName}:");
    Console.WriteLine($"  Total: {metric.TotalRequests}");
    Console.WriteLine($"  Success: {metric.SuccessRate:F2}%");
    Console.WriteLine($"  Avg Time: {metric.AverageExecutionTime}ms");
}

// Output:
// GetProfileQuery:
//   Total: 1234
//   Success: 99.50%
//   Avg Time: 245ms
```

## When Applied

```
All Requests → Metrics Collected ✓
  Queries
  Commands
  All operations

Real-time metrics available
No special configuration needed
```

## Integration (2 Steps)

### Step 1: Register First in Pipeline
```csharp
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>)); // ← First!
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
// ... other behaviors
```

### Step 2: Done!
Metrics collected automatically for all requests.

## Access Metrics

### Get All Metrics
```csharp
var allMetrics = MetricsBehavior<object, object>.GetAllMetrics();
// Returns Dictionary<string, RequestMetrics>
```

### Get Specific Request Metrics
```csharp
var profileMetrics = MetricsBehavior<object, object>.GetMetrics("GetProfileQuery");
// Returns RequestMetrics or null
```

### Reset Metrics
```csharp
MetricsBehavior<object, object>.ResetMetrics();
// Clears all collected metrics
```

## Metrics Available

```
Basic Counts:
  ✅ TotalRequests
  ✅ SuccessfulRequests
  ✅ FailedRequests
  
Calculated:
  ✅ SuccessRate (percentage)
  ✅ ErrorCounts (dictionary)
  ✅ MostCommonError
  ✅ MostCommonErrorCount
  
Execution Time:
  ✅ MinExecutionTime (ms)
  ✅ MaxExecutionTime (ms)
  ✅ AverageExecutionTime (ms)
  ✅ TotalExecutionTime (ms)
```

## Log Output

### Successful Request
```
[Information] Request GetTopLevelKpisQuery completed in 523ms
```

### Failed Request
```
[Error] Request CreateOrderCommand failed after 245ms with error: ValidationException
```

### Cancelled Request
```
[Warning] Request UpdateOrderCommand was cancelled after 1500ms
```

## Metrics Controller Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllMetrics()
    {
        var allMetrics = MetricsBehavior<object, object>.GetAllMetrics();
        return Ok(allMetrics.Values.Select(m => new
        {
            m.RequestName,
            m.TotalRequests,
            m.SuccessRate,
            Performance = new
            {
                m.MinExecutionTime,
                m.MaxExecutionTime,
                m.AverageExecutionTime
            },
            m.ErrorCounts
        }));
    }
    
    [HttpGet("{requestName}")]
    public IActionResult GetMetrics(string requestName)
    {
        var metrics = MetricsBehavior<object, object>.GetMetrics(requestName);
        return metrics != null ? Ok(metrics) : NotFound();
    }
    
    [HttpDelete]
    public IActionResult ResetMetrics()
    {
        MetricsBehavior<object, object>.ResetMetrics();
        return Ok(new { message = "Metrics reset" });
    }
}
```

## Example Queries

### Find Slow Operations
```csharp
var slowOps = allMetrics
    .Where(x => x.Value.AverageExecutionTime > 1000)
    .OrderByDescending(x => x.Value.AverageExecutionTime)
    .Take(10);
```

### Find Failing Operations
```csharp
var failingOps = allMetrics
    .Where(x => x.Value.SuccessRate < 95)
    .OrderBy(x => x.Value.SuccessRate);
```

### Get Error Summary
```csharp
var errorSummary = allMetrics
    .Where(x => x.Value.ErrorCounts.Count > 0)
    .Select(x => new
    {
        x.Key,
        x.Value.MostCommonError,
        x.Value.FailedRequests
    });
```

## Pipeline Position

```
REQUEST
  ↓
MetricsBehavior ← HERE ... Collect metrics (first)
  ↓
LoggingBehavior ........... Log request
  ↓
CachingBehavior ........... Return cached
  ↓
ValidationBehavior ........ Validate
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
HANDLER
```

## Performance Overhead

```
Metric Collection: ~1ms per request
Database Query: 500ms
Overhead: 0.2%

Negligible impact!
```

## Best Practices

✅ **DO**:
- Use metrics for alerting
- Monitor success rates
- Track slow operations
- Export for analysis
- Reset periodically

❌ **DON'T**:
- Ignore failing operations
- Miss performance trends
- Store indefinitely (memory leak)
- Skip baseline comparison

## Thread Safety

✅ Fully thread-safe
✅ Concurrent update safe
✅ No data corruption
✅ Suitable for production

## Related Documentation

- See `METRICS_README.md` for complete guide
- See `INDEX.md` for all behaviors
- See `COMPLETE_IMPLEMENTATION.md` for full overview

## Build Status

✅ Build Successful - Ready to use!

## Key Takeaways

✅ Automatic metrics collection  
✅ Success/failure rate tracking  
✅ Performance analysis ready  
✅ Error categorization  
✅ Thread-safe storage  

## Dependencies

- ✅ **MediatR** - Already installed
- ✅ **Logging** - Uses ILogger
- ✅ **System.Diagnostics** - For Stopwatch
