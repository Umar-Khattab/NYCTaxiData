# Timeout Behavior Implementation Guide

## Overview
The `TimeoutBehavior` is a MediatR pipeline behavior that enforces timeout limits on request execution. Using the Polly resilience library, it prevents long-running operations from consuming resources indefinitely by automatically canceling requests that exceed specified timeout durations.

## Components

### TimeoutBehavior
Located in: `NYCTaxiData.Application\Behaviors\TimeoutBehavior.cs`

This behavior wraps every request with a timeout policy.

**Features:**
- Configurable timeout durations per request type
- Different timeout values for queries vs. commands
- Longer timeouts for AI/heavy operations
- Optimistic timeout strategy from Polly
- Comprehensive logging of timeout violations
- Automatic cancellation via CancellationToken

## How It Works

### Timeout Strategy

The behavior uses Polly's **Optimistic Timeout Strategy**:
- Doesn't aggressively abort the task
- Uses CancellationToken for graceful cancellation
- Allows tasks to clean up resources
- Safer for database operations

```csharp
var timeoutPolicy = Policy.TimeoutAsync<TResponse>(
    TimeSpan.FromSeconds(timeoutSeconds),
    TimeoutStrategy.Optimistic);
```

### Execution Flow

```
1. Request arrives
    ↓
2. TimeoutBehavior determines timeout duration
    ↓
3. Creates timeout policy with duration
    ↓
4. Request starts executing with timeout limit
    ↓
5a. If completes within timeout → Return response ✓
5b. If exceeds timeout → Throw OperationCanceledException ✗
    ↓
6. Log result
    ↓
7. Return or throw
```

## Default Timeout Configuration

### Queries (Faster Operations)
```csharp
"GetProfileQuery" => 10 seconds
"GetActiveFleetQuery" => 15 seconds
"GetAllZonesQuery" => 15 seconds
"GetTopLevelKpisQuery" => 20 seconds
"GetDemandForecastQuery" => 25 seconds
"GetLiveDispatchFeedQuery" => 15 seconds
```

### Commands (Standard Operations)
```csharp
"LoginCommand" => 15 seconds
"RegisterCommand" => 20 seconds
"UpdateSystemThresholdsCommand" => 30 seconds
"StartTripCommand" => 20 seconds
"EndTripCommand" => 25 seconds
"ManualDispatchCommand" => 20 seconds
```

### AI Operations (Long-Running)
```csharp
"RunOperationalSimulationCommand" => 60 seconds
"RunStrategicSimulationCommand" => 90 seconds
"TriggerModelRetrainingCommand" => 120 seconds
"ProcessVoiceAssistantQuery" => 45 seconds
```

### Default for Unknown Operations
```csharp
DefaultTimeoutSeconds = 30 seconds
```

## Integration Steps

### Step 1: Register in MediatR Configuration
```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Add TimeoutBehavior (typically after validation, before transaction)
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TimeoutBehavior<,>)); // ← NEW
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
});
```

### Step 2: That's It!
TimeoutBehavior automatically applies to all requests.

## Customizing Timeouts

### Method 1: Edit TimeoutBehavior (Global Changes)
```csharp
private int GetTimeoutForRequest(string requestName)
{
    return requestName switch
    {
        "MySlowQuery" => 60,           // Give it more time
        "MyFastQuery" => 5,             // Reduce timeout
        "MyCommand" => 45,              // Custom timeout
        _ => DefaultTimeoutSeconds      // Default fallback
    };
}
```

### Method 2: Create Custom Timeout Attribute (Future Enhancement)
```csharp
[TimeoutSeconds(120)]  // Custom timeout for this specific request
public record SlowOperationCommand : IRequest<Unit>;
```

## Log Output Examples

### Successful Execution Within Timeout
```
[Debug] Request GetProfileQuery will be executed with timeout of 10 seconds
// Request completes successfully
[Information] Request completed successfully: GetProfileQuery - Execution time: 245ms
```

### Timeout Exceeded
```
[Debug] Request RunOperationalSimulationCommand will be executed with timeout of 60 seconds
[Error] Request RunOperationalSimulationCommand exceeded timeout of 60 seconds
System.OperationCanceledException: A task was canceled.
```

### Long-Running AI Operation
```
[Debug] Request TriggerModelRetrainingCommand will be executed with timeout of 120 seconds
// Request runs for 95 seconds
[Information] Request completed successfully: TriggerModelRetrainingCommand - Execution time: 95234ms
```

## Error Handling

### When Timeout Occurs
```csharp
catch (OperationCanceledException ex)
{
    _logger.LogError(
        ex,
        "Request {RequestName} exceeded timeout of {TimeoutSeconds} seconds",
        requestName,
        timeoutSeconds);
    
    throw; // Propagate to exception handler
}
```

### Global Exception Handler Integration
```csharp
if (exception is OperationCanceledException)
{
    httpContext.Response.StatusCode = 408;  // Request Timeout
    await httpContext.Response.WriteAsJsonAsync(new
    {
        message = "Request exceeded timeout limit",
        statusCode = 408
    });
    return true;
}
```

## Best Practices

### 1. **Set Realistic Timeouts**
```csharp
// ✅ GOOD - Realistic time based on typical execution
"GetProfileQuery" => 10,      // Simple profile fetch
"RunSimulation" => 120,        // Heavy computation

// ❌ BAD - Too short or too long
"GetProfileQuery" => 1,        // Unrealistic, will always timeout
"SimpleQuery" => 3600,         // One hour, defeats the purpose
```

### 2. **Distinguish Query vs. Command Timeouts**
```csharp
// ✅ GOOD - Queries are shorter, commands can take longer
"GetDataQuery" => 15,
"ProcessDataCommand" => 45,

// ❌ BAD - Same timeout for everything
"GetDataQuery" => 30,
"ProcessDataCommand" => 30,
```

### 3. **Account for Special Operations**
```csharp
// ✅ GOOD - AI/heavy operations get more time
"GenerateReportCommand" => 60,
"TriggerRetrainingCommand" => 120,
"RunSimulation" => 90,

// ❌ BAD - All operations treated equally
"GetBasicInfoQuery" => 30,
"TriggerRetrainingCommand" => 30,  // Not enough time!
```

### 4. **Monitor and Adjust**
```csharp
// Check logs for timeouts and adjust accordingly
// If you see frequent timeouts, increase the timeout
// If timeouts are rare, operation likely completes well within limit
```

### 5. **Use Optimistic Strategy**
```csharp
// ✅ Already implemented - Polly.TimeoutStrategy.Optimistic
// - Graceful cancellation via CancellationToken
// - Allows cleanup and resource release
// - Safer for database operations
```

## Testing Timeout Behavior

### Test 1: Request Completes Within Timeout
```csharp
[TestMethod]
public async Task Handle_RequestCompletesWithinTimeout_ReturnsResponse()
{
    // Arrange
    var command = new FastCommand();
    var behavior = new TimeoutBehavior<FastCommand, Unit>(_mockLogger.Object);
    
    // Act
    var result = await behavior.Handle(
        command, 
        async () => { await Task.Delay(100); return Unit.Value; },
        CancellationToken.None);
    
    // Assert
    Assert.IsNotNull(result);
}
```

### Test 2: Request Exceeds Timeout
```csharp
[TestMethod]
[ExpectedException(typeof(OperationCanceledException))]
public async Task Handle_RequestExceedsTimeout_ThrowsException()
{
    // Arrange
    var command = new SlowCommand();
    var behavior = new TimeoutBehavior<SlowCommand, Unit>(_mockLogger.Object);
    
    // Act - This request takes longer than configured timeout
    await behavior.Handle(
        command,
        async () => { await Task.Delay(5000); return Unit.Value; },
        CancellationToken.None);
}
```

## Pipeline Position

The recommended position for TimeoutBehavior is:

```
1. LoggingBehavior         ← Log all requests
2. CachingBehavior         ← Check cache
3. ValidationBehavior      ← Validate data
4. AuthorizationBehavior   ← Check permissions
5. TimeoutBehavior ← NEW   ← Enforce timeout
6. TransactionBehavior     ← Database transaction
```

**Why this position?**
- After validation (no point timing invalid requests)
- After authorization (don't timeout blocked requests)
- Before transaction (ensure operation completes or cancels before DB work)

## HTTP Status Codes

| Status Code | Meaning | When |
|-------------|---------|------|
| **408** | Request Timeout | Request exceeded timeout limit |
| **504** | Gateway Timeout | External dependency timed out |

```csharp
// In exception handler
if (exception is OperationCanceledException)
{
    httpContext.Response.StatusCode = 408;  // Request Timeout
}
```

## Monitoring and Metrics

### Key Metrics to Track
1. **Timeout rate** - How often requests are timing out
2. **Request duration** - Compare against timeout values
3. **Timeout by operation** - Which operations timeout most often

### Query Examples
```bash
# Find all timeout errors
grep "exceeded timeout" logs/log-*.txt

# Get timeout statistics
grep "exceeded timeout" logs/log-*.txt | wc -l

# Find operations frequently timing out
grep "exceeded timeout" logs/log-*.txt | sed 's/.*Request \([^ ]*\).*/\1/' | sort | uniq -c
```

## Polly Documentation

### TimeoutStrategy Options
```csharp
// Optimistic (Recommended)
Policy.TimeoutAsync<T>(TimeSpan, TimeoutStrategy.Optimistic)
// Uses CancellationToken, allows cleanup

// Pessimistic
Policy.TimeoutAsync<T>(TimeSpan, TimeoutStrategy.Pessimistic)
// Forcefully aborts task, potential resource leak
```

### Circuit Breaker Integration (Future)
Combine with Polly's circuit breaker for additional resilience:

```csharp
var policy = Policy
    .Handle<OperationCanceledException>()
    .CircuitBreakerAsync<TResponse>(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30));
```

## Common Issues & Solutions

### Issue: Too Many Timeouts
**Symptoms**: Frequent 408 errors in logs

**Solutions**:
1. Increase timeout values for affected operations
2. Optimize query/command performance
3. Check database/external service performance
4. Review logs for bottlenecks

### Issue: Operations Randomly Timeout
**Symptoms**: Same operation sometimes times out, sometimes doesn't

**Solutions**:
1. Increase timeout to account for variance
2. Optimize for worst-case performance
3. Implement caching for expensive operations
4. Scale infrastructure if at capacity

### Issue: Timeout Not Being Applied
**Symptoms**: No timeout errors despite long-running operations

**Solutions**:
1. Verify TimeoutBehavior is registered in MediatR
2. Check log levels - Debug logs may not be visible
3. Verify request name is recognized by switch statement
4. Check for exceptions being swallowed

## Related Files

- **TimeoutBehavior**: `NYCTaxiData.Application\Behaviors\TimeoutBehavior.cs`
- **LoggingBehavior**: For logging timeout events
- **GlobalExceptionHandler**: For handling timeout exceptions
- **Polly Package**: `Polly` v8.6.6

## Configuration Values Reference

```
Quick Queries (5-15 seconds):
  - Get single entity: 10 seconds
  - Get list data: 15 seconds

Standard Commands (15-30 seconds):
  - Authentication: 15 seconds
  - Database updates: 20-25 seconds

Heavy Operations (30+ seconds):
  - Bulk processing: 45 seconds
  - Simulations: 60-90 seconds
  - Model training: 120+ seconds
```

## Summary

The **TimeoutBehavior** provides:

✅ Automatic timeout enforcement on all requests  
✅ Configurable timeouts per operation type  
✅ Graceful cancellation via Polly  
✅ Comprehensive logging of violations  
✅ Prevents resource exhaustion  
✅ Improves system responsiveness  

It's a critical component for building resilient APIs that prevent cascading failures from slow operations.
