# Performance Behavior - Quick Reference

## What It Does

Monitors request performance in real-time:
- **Detects slow operations** - Logs warnings for slow requests
- **Tracks trends** - Compares current vs previous period
- **Alerts on degradation** - Warns when performance degrades >20%
- **Categorizes severity** - SLOW, WARNING, CRITICAL levels
- **Analyzes trends** - Rolling window of last 100 measurements

## Quick Example

```csharp
// Automatically monitored for all requests
GetProfileQuery: 245ms (current avg) vs 200ms (previous)
  → Degradation: 22.5% 
  → Alert: Performance degraded by 22.5%

GetTopLevelKpisQuery: 523ms
  → Exceeds 500ms threshold
  → Alert: SLOW - 523ms (threshold: 500ms)

RunSimulationCommand: 8500ms
  → Exceeds 5000ms critical threshold
  → Alert: CRITICAL - 8500ms (threshold: 5000ms)
```

## Performance Thresholds

```
Queries:
  Slow threshold ................ 500ms
  Significantly slow ............ 1000ms (2x)
  Critical ...................... 5000ms

Commands:
  Slow threshold ................ 1000ms
  Significantly slow ............ 2000ms (2x)
  Critical ...................... 5000ms

Degradation:
  Alert threshold ............... 20%
```

## Integration (2 Steps)

### Step 1: Register Behavior
```csharp
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>)); // ← NEW
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
// ... other behaviors
```

### Step 2: Done!
Performance monitoring works automatically.

## Access Performance Data

### Get Slow Operations
```csharp
var slowOps = PerformanceBehavior<object, object>.GetSlowOperations();
foreach (var (name, avgTime, maxTime) in slowOps)
{
    Console.WriteLine($"{name}: {avgTime}ms avg, {maxTime}ms max");
}
```

### Get Degrading Operations
```csharp
var degrading = PerformanceBehavior<object, object>.GetDegradingOperations();
foreach (var (name, degradationPercent) in degrading)
{
    Console.WriteLine($"{name}: {degradationPercent:F2}% degraded");
}
```

### Get Specific Performance History
```csharp
var history = PerformanceBehavior<object, object>.GetPerformanceHistory("GetProfileQuery");
Console.WriteLine($"Current avg: {history.CurrentPeriodAverage}ms");
Console.WriteLine($"Previous avg: {history.PreviousPeriodAverage}ms");
Console.WriteLine($"Min/Max: {history.MinExecutionTime}/{history.MaxExecutionTime}ms");
```

## Log Output Examples

### Slow Operation
```
[Warning] SLOW: Request GetProfileQuery exceeded threshold - 523ms (threshold: 500ms)
```

### Significantly Slow
```
[Warning] WARNING: Request CreateOrderCommand is significantly slow - 2100ms (threshold: 1000ms)
```

### Critical Issue
```
[Error] CRITICAL: Request RunSimulationCommand is VERY SLOW - 8500ms (threshold: 5000ms)
```

### Degradation Alert
```
[Warning] DEGRADATION: Request GetProfileQuery performance degraded by 22.5% (200ms → 245ms)
```

## Pipeline Position

```
REQUEST
  ↓
[1] MetricsBehavior ........... Collect metrics
[2] PerformanceBehavior ← HERE ... Monitor performance (alert on slow/degradation)
[3] LoggingBehavior .......... Log request
[4] CachingBehavior .......... Return cached
[5] ValidationBehavior ....... Validate
[6] AuthorizationBehavior .... Check permissions
[7] IdempotencyBehavior ...... Prevent duplicates
[8] RetryBehavior ........... Retry failures
[9] TimeoutBehavior .......... Enforce timeout
[10] TransactionBehavior ..... Manage transaction
```

## Use Cases

### Find Bottlenecks
```csharp
var slowOps = PerformanceBehavior<object, object>.GetSlowOperations();
// Optimize: add index, cache, or simplify query
```

### Monitor SLA Compliance
```csharp
var violating = allOps.Where(x => x.AverageTime > SlaTarget);
// Alert or escalate if exceeding SLA
```

### Detect Regression
```csharp
var degrading = PerformanceBehavior<object, object>.GetDegradingOperations();
// Investigation needed - performance got worse
```

### Track Optimization Success
```csharp
// Before optimization
GetProfileQuery: 245ms avg (degrading)

// After optimization
GetProfileQuery: 180ms avg (not degrading) ✓
```

## Severity Levels

```
SLOW .......... Yellow   500ms (query) / 1000ms (command)
WARNING ...... Orange   1000ms (query) / 2000ms (command)
CRITICAL ..... Red      5000ms (any)
```

## Best Practices

✅ **DO**:
- Monitor regularly
- Set up alerts
- Track trends
- Export to monitoring tool
- Optimize degrading operations

❌ **DON'T**:
- Ignore performance warnings
- Let degradation continue
- Skip monitoring setup
- Optimize without data

## Related Documentation

- See `PERFORMANCE_README.md` for complete guide
- See `METRICS_QUICK_REF.md` for metrics collection
- See `INDEX.md` for all behaviors

## Build Status

✅ Build Successful - Ready to use!

## Key Takeaways

✅ Real-time performance monitoring  
✅ Slow operation detection  
✅ Degradation alerts  
✅ Trend analysis  
✅ Thread-safe storage  

## Dependencies

- ✅ **MediatR** - Already installed
- ✅ **Logging** - Uses ILogger
- ✅ **System.Diagnostics** - For Stopwatch
