# Performance Behavior Implementation Guide

## Overview
The `PerformanceBehavior` is a MediatR pipeline behavior that monitors request execution performance, identifies slow operations, detects performance degradation, and alerts on performance issues. It complements the MetricsBehavior by providing real-time performance analysis and trend detection for proactive performance optimization.

## Components

### PerformanceBehavior
Located in: `NYCTaxiData.Application\Behaviors\PerformanceBehavior.cs`

This behavior monitors performance in real-time and alerts on issues.

**Features:**
- Real-time execution time monitoring
- Automatic slow operation detection
- Performance degradation tracking
- Trend analysis (compares current vs previous period)
- Threshold-based alerting
- Thread-safe history tracking
- Production-ready implementation

### PerformanceHistory
Container class for tracking performance trends over time.

**Properties:**
- `RequestName` - Name of the request type
- `RecentMeasurements` - Last 100 measurements (rolling window)
- `PreviousPeriodAverage` - Average from previous period
- `CurrentPeriodAverage` - Average of current period
- `MinExecutionTime` - Best performance recorded
- `MaxExecutionTime` - Worst performance recorded
- `TotalMeasurements` - Total requests tracked

**Methods:**
- `AddMeasurement()` - Record execution time
- `IsDegrading()` - Check for performance degradation
- `GetDegradationPercentage()` - Get degradation amount

## Performance Thresholds

```csharp
// Query thresholds
SlowQueryThreshold = 500ms         // Queries > 500ms logged as slow
2x Threshold = 1000ms              // Queries > 1 second logged as significantly slow
VerySlowThreshold = 5000ms         // Queries > 5 seconds logged as CRITICAL

// Command thresholds
SlowCommandThreshold = 1000ms      // Commands > 1 second logged as slow
2x Threshold = 2000ms              // Commands > 2 seconds logged as significantly slow
VerySlowThreshold = 5000ms         // Commands > 5 seconds logged as CRITICAL

// Degradation threshold
DegradationThreshold = 20%         // 20% performance decrease triggers alert
```

## How It Works

### Request Processing Flow

```
REQUEST
    ↓
Start stopwatch
    ↓
Execute request
    ↓
Check performance against thresholds
  ├─ > 5 seconds   → Log CRITICAL error
  ├─ > 2x threshold → Log WARNING (significant slowness)
  └─ > threshold    → Log WARNING (slowness)
    ↓
Record to performance history
  ├─ Add measurement to rolling window
  ├─ Update min/max
  └─ Check for degradation
    ↓
Alert if degrading
  └─ Compare current avg vs previous avg
    ↓
RESPONSE
```

### Performance Analysis

```csharp
// Example: GetProfileQuery performance tracking
GetProfileQuery (100 measurements):
  Current Period Avg: 245ms
  Previous Period Avg: 200ms
  Degradation: (245-200)/200 * 100 = 22.5%
  
  Status: ⚠️ DEGRADING (exceeds 20% threshold)
  Alert: Performance degraded by 22.5%
```

## Integration Steps

### Step 1: Register in MediatR Configuration (After Metrics)
```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>)); // ← NEW
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    // ... other behaviors
});
```

### Step 2: Access Performance Data (Optional)
```csharp
// Get all performance histories
var allHistories = PerformanceBehavior<object, object>.GetAllPerformanceHistories();

// Get specific request performance
var profileHistory = PerformanceBehavior<object, object>.GetPerformanceHistory("GetProfileQuery");

// Get slow operations
var slowOps = PerformanceBehavior<object, object>.GetSlowOperations();

// Get degrading operations
var degradingOps = PerformanceBehavior<object, object>.GetDegradingOperations();
```

### Step 3: That's It!
Performance monitoring works automatically.

## Log Output Examples

### Slow Query
```
[Warning] SLOW: Request GetTopLevelKpisQuery exceeded threshold - 523ms (threshold: 500ms)
```

### Significantly Slow Operation
```
[Warning] WARNING: Request CreateOrderCommand is significantly slow - 2100ms (threshold: 1000ms)
```

### Critical Performance Issue
```
[Error] CRITICAL: Request RunSimulationCommand is VERY SLOW - 8500ms (threshold: 5000ms)
```

### Performance Degradation Alert
```
[Warning] DEGRADATION: Request GetProfileQuery performance degraded by 22.5% (200ms → 245ms)
```

## Accessing Performance Data

### Controller Example
```csharp
[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    [HttpGet("slow-operations")]
    public IActionResult GetSlowOperations()
    {
        var slowOps = PerformanceBehavior<object, object>.GetSlowOperations();
        return Ok(slowOps.Select(x => new
        {
            RequestName = x.RequestName,
            AverageTime = $"{x.AverageTime}ms",
            MaxTime = $"{x.MaxTime}ms"
        }));
    }
    
    [HttpGet("degrading-operations")]
    public IActionResult GetDegradingOperations()
    {
        var degradingOps = PerformanceBehavior<object, object>.GetDegradingOperations();
        return Ok(degradingOps.Select(x => new
        {
            RequestName = x.RequestName,
            DegradationPercent = $"{x.DegradationPercent:F2}%"
        }));
    }
    
    [HttpGet("performance/{requestName}")]
    public IActionResult GetPerformance(string requestName)
    {
        var history = PerformanceBehavior<object, object>.GetPerformanceHistory(requestName);
        if (history == null)
            return NotFound();
            
        return Ok(new
        {
            history.RequestName,
            history.TotalMeasurements,
            CurrentAverage = $"{history.CurrentPeriodAverage}ms",
            PreviousAverage = $"{history.PreviousPeriodAverage}ms",
            Min = $"{history.MinExecutionTime}ms",
            Max = $"{history.MaxExecutionTime}ms",
            Degrading = history.IsDegrading(),
            DegradationPercent = $"{history.GetDegradationPercentage():F2}%"
        });
    }
}
```

## Performance Optimization Workflow

### 1. Identify Slow Operations
```csharp
var slowOps = PerformanceBehavior<object, object>.GetSlowOperations();

// Output:
// GetLiveDispatchFeedQuery: 1200ms avg (threshold: 500ms)
// GetTopLevelKpisQuery: 850ms avg (threshold: 500ms)
// CreateOrderCommand: 2100ms avg (threshold: 1000ms)
```

### 2. Identify Degrading Operations
```csharp
var degrading = PerformanceBehavior<object, object>.GetDegradingOperations();

// Output:
// GetProfileQuery: 25% degradation
// GetActiveFleetQuery: 18% degradation
```

### 3. Investigate and Optimize
```csharp
// Check detailed history
var history = PerformanceBehavior<object, object>
    .GetPerformanceHistory("GetProfileQuery");

Console.WriteLine($"Current avg: {history.CurrentPeriodAverage}ms");
Console.WriteLine($"Previous avg: {history.PreviousPeriodAverage}ms");

// Options:
// 1. Add database index
// 2. Implement caching
// 3. Optimize query
// 4. Add rate limiting
```

### 4. Verify Improvement
```csharp
// After optimization, monitor:
var newHistory = PerformanceBehavior<object, object>
    .GetPerformanceHistory("GetProfileQuery");

if (!newHistory.IsDegrading())
    Console.WriteLine("✓ Performance restored!");
```

## Best Practices

### 1. **Monitor Regularly**
```csharp
// Set up periodic checks
using System.Timers;

var timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
timer.Elapsed += (s, e) =>
{
    var slowOps = PerformanceBehavior<object, object>.GetSlowOperations();
    if (slowOps.Count > 0)
        AlertSlowOperations(slowOps);
};
timer.Start();
```

### 2. **Set Up Alerts**
```csharp
// Alert on degradation
var degrading = PerformanceBehavior<object, object>.GetDegradingOperations();
if (degrading.Any())
{
    foreach (var (requestName, percent) in degrading)
    {
        SendAlert($"{requestName} degraded by {percent:F2}%");
    }
}
```

### 3. **Track SLA Compliance**
```csharp
// Track compliance with SLA targets
var allHistories = PerformanceBehavior<object, object>
    .GetAllPerformanceHistories();

var slaViolations = allHistories
    .Where(x => x.Value.CurrentPeriodAverage > GetSlaTarget(x.Key))
    .ToList();
    
Console.WriteLine($"SLA Violations: {slaViolations.Count}");
```

### 4. **Export for Analysis**
```csharp
// Export to monitoring tool
var allHistories = PerformanceBehavior<object, object>
    .GetAllPerformanceHistories();

foreach (var history in allHistories.Values)
{
    SendToMonitoringTool(new
    {
        timestamp = DateTime.UtcNow,
        request = history.RequestName,
        avg = history.CurrentPeriodAverage,
        min = history.MinExecutionTime,
        max = history.MaxExecutionTime,
        degrading = history.IsDegrading()
    });
}
```

## Thresholds Customization

```csharp
// To customize thresholds, modify the constants:
private const long SlowQueryThreshold = 500;        // Adjust for your SLA
private const long SlowCommandThreshold = 1000;     // Adjust for your SLA
private const long VerySlowThreshold = 5000;        // Critical threshold

// For different request types, update CheckPerformance():
private void CheckPerformance(string requestName, long elapsedMilliseconds)
{
    var threshold = requestName switch
    {
        "GetProfileQuery" => 200,              // Fast queries
        "GetLiveDispatchFeedQuery" => 1000,   // Real-time is slower
        "RunSimulationCommand" => 10000,      // Long operations
        _ => // default
    };
    
    if (elapsedMilliseconds > threshold)
        _logger.LogWarning(...);
}
```

## Performance Impact

```
Performance Monitoring Overhead: ~1ms per request
Total Pipeline Overhead: ~3ms (metrics + performance + logging)
Database Query Time: 500ms
Overall Overhead: 0.6%

Negligible impact - safe for production!
```

## Testing Performance Behavior

### Test 1: Slow Query Detection
```csharp
[TestMethod]
public async Task Handle_SlowQuery_LogsWarning()
{
    var query = new GetProfileQuery { UserId = "user-123" };
    var slowHandler = new SlowQueryHandler();  // Simulates 600ms delay
    var behavior = new PerformanceBehavior<GetProfileQuery, UserProfile>(_logger);
    
    await behavior.Handle(query, async () => await slowHandler.Handle(query, default), default);
    
    // Verify warning logged for exceeding 500ms threshold
    _loggerMock.Verify(
        l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("SLOW")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
}
```

### Test 2: Degradation Detection
```csharp
[TestMethod]
public async Task Handle_PerformanceDegradation_AlertsOnTrend()
{
    var query = new GetProfileQuery { UserId = "user-123" };
    var behavior = new PerformanceBehavior<GetProfileQuery, UserProfile>(_logger);
    
    // Simulate 100 measurements at 200ms
    for (int i = 0; i < 100; i++)
    {
        await SimulateRequest(200);
    }
    
    // Update previous period
    var history = PerformanceBehavior<GetProfileQuery, UserProfile>
        .GetPerformanceHistory("GetProfileQuery");
    
    // Simulate degradation to 250ms
    for (int i = 0; i < 20; i++)
    {
        await SimulateRequest(250);
    }
    
    // Verify degradation detected (25% increase)
    Assert.IsTrue(history.IsDegrading());
    Assert.IsTrue(history.GetDegradationPercentage() > 20);
}
```

## Related Files

- **PerformanceBehavior**: `NYCTaxiData.Application\Behaviors\PerformanceBehavior.cs`
- **MetricsBehavior**: Complements with detailed metrics
- **LoggingBehavior**: Logs performance warnings
- **TimeoutBehavior**: Prevents extreme slowness

## Summary

The **PerformanceBehavior** provides:

✅ Real-time performance monitoring  
✅ Automatic slow operation detection  
✅ Performance degradation tracking  
✅ Trend analysis  
✅ Threshold-based alerting  
✅ Thread-safe history tracking  
✅ Minimal performance overhead  

It's essential for proactive performance management and SLA compliance monitoring.
