# Retry Behavior Implementation Guide

## Overview
The `RetryBehavior` is a MediatR pipeline behavior that automatically retries failed requests on transient errors. Using the Polly resilience library, it implements exponential backoff to gracefully handle temporary failures like network issues, database connection problems, and service timeouts.

## Components

### RetryBehavior
Located in: `NYCTaxiData.Application\Behaviors\RetryBehavior.cs`

This behavior wraps every request with a retry policy for transient failures.

**Features:**
- Automatic detection of transient vs. permanent errors
- Exponential backoff (1s, 2s, 4s, 8s, etc.)
- Configurable retry counts per operation type
- Skips retry for validation/authorization errors
- Logs all retry attempts with timestamps
- Graceful failure after max retries exceeded

## How It Works

### Transient Error Detection

The behavior distinguishes between **transient** (recoverable) and **permanent** (non-recoverable) errors:

**Transient (Retried)**:
```
✅ Database connection failures
✅ Network timeouts
✅ Temporary service unavailability
✅ HTTP request exceptions
✅ IOException
✅ SQL/PostgreSQL connection errors
```

**Permanent (Not Retried)**:
```
❌ ValidationException
❌ UnauthorizedException
❌ OperationCanceledException
❌ Business logic errors
```

### Exponential Backoff Strategy

```
Attempt 1: Immediate
Attempt 2: Wait 1 second  (2^0)
Attempt 3: Wait 2 seconds (2^1)
Attempt 4: Wait 4 seconds (2^2)

Total: 7 seconds for 4 attempts
```

### Execution Flow

```
1. Request arrives
    ↓
2. RetryBehavior checks if it's retryable
    ↓
3. Execute request with retry policy
    ↓
4a. Success → Return response ✓
4b. Transient error:
    ├─ Log warning with retry attempt
    ├─ Wait (exponential backoff)
    └─ Retry (up to max count)
4c. Permanent error → Throw immediately ✗
    ↓
5. After max retries → Throw exception
```

## Default Retry Configuration

### Queries (Idempotent - Safe to Retry)
```csharp
"GetProfileQuery" => 3 retries
"GetActiveFleetQuery" => 3 retries
"GetAllZonesQuery" => 3 retries
"GetTopLevelKpisQuery" => 2 retries
"GetDemandForecastQuery" => 2 retries
"GetLiveDispatchFeedQuery" => 3 retries
```

### Commands (Be Cautious - May Have Side Effects)
```csharp
"LoginCommand" => 2 retries
"RegisterCommand" => 2 retries
"UpdateSystemThresholdsCommand" => 2 retries
"UpdateDriverStatusCommand" => 2 retries
"SyncOfflineTripsCommand" => 3 retries
```

### External Integration Commands (Network-Based)
```csharp
"SendOtpCommand" => 3 retries (network-based)
"ProcessVoiceAssistantQuery" => 2 retries (API-based)
```

### Long-Running Operations (Time-Consuming)
```csharp
"RunOperationalSimulationCommand" => 1 retry
"RunStrategicSimulationCommand" => 1 retry
"TriggerModelRetrainingCommand" => 1 retry
```

### Default for Unknown Operations
```csharp
Unknown request type => 3 retries (DefaultRetryCount)
```

## Integration Steps

### Step 1: Register in MediatR Configuration
```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Add RetryBehavior (after validation, before timeout typically)
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>)); // ← NEW
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TimeoutBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
});
```

### Step 2: That's It!
RetryBehavior automatically handles retries for all requests.

## Log Output Examples

### Successful Request (No Retries Needed)
```
[Debug] Request GetProfileQuery will be executed with retry policy (max 3 retries)
[Information] Request completed successfully: GetProfileQuery - Execution time: 245ms
```

### Request with One Retry
```
[Debug] Request GetActiveFleetQuery will be executed with retry policy (max 3 retries)
[Warning] Request GetActiveFleetQuery failed with exception SqlException: Connection timeout. Retrying in 1000ms (Attempt 1/3)
[Information] Request completed successfully: GetActiveFleetQuery - Execution time: 1342ms
```

### Request Failed After All Retries
```
[Debug] Request SyncOfflineTripsCommand will be executed with retry policy (max 3 retries)
[Warning] Request SyncOfflineTripsCommand failed with exception SqlException: Connection closed. Retrying in 1000ms (Attempt 1/3)
[Warning] Request SyncOfflineTripsCommand failed with exception SqlException: Connection closed. Retrying in 2000ms (Attempt 2/3)
[Warning] Request SyncOfflineTripsCommand failed with exception SqlException: Connection closed. Retrying in 4000ms (Attempt 3/3)
[Error] Request SyncOfflineTripsCommand failed after 3 retries. Final error: Connection permanently closed
```

### Request Failed on Permanent Error (No Retry)
```
[Debug] Request LoginCommand will be executed with retry policy (max 2 retries)
[Error] Request LoginCommand failed after 2 retries. Final error: Invalid credentials
```

## Customizing Retry Behavior

### Method 1: Change Retry Counts
```csharp
private int GetRetryCountForRequest(string requestName)
{
    return requestName switch
    {
        "MyFastQuery" => 5,              // Increase retries
        "MySlowCommand" => 1,             // Decrease retries
        "CriticalOperation" => 0,         // No retries
        _ => DefaultRetryCount
    };
}
```

### Method 2: Change Transient Error Detection
```csharp
private bool IsTransientError(Exception exception)
{
    // Add custom transient error logic
    if (exception.Message.Contains("MyCustomTransientError"))
        return true;
    
    // ... existing logic ...
}
```

### Method 3: Create Custom Retry Attribute (Future Enhancement)
```csharp
[Retry(maxAttempts: 5, backoffMultiplier: 2)]
public record CriticalQuery : IRequest<Result>;
```

## Best Practices

### 1. **Only Retry Idempotent Operations**
```csharp
// ✅ SAFE - Queries don't have side effects
"GetDataQuery" => 3,

// ⚠️ RISKY - Updates have side effects
"UpdateDataCommand" => 2,  // Keep retries low

// ❌ DANGEROUS - Payment command
"ProcessPaymentCommand" => 0,  // Don't retry
```

### 2. **Match Retry Count to Operation Type**
```csharp
// Network-based (external API calls)
"CallExternalApiCommand" => 3,

// Database operations
"UpdateDatabaseCommand" => 2,

// Long-running (already slow)
"HeavyComputationCommand" => 1,
```

### 3. **Monitor Retry Rates**
```csharp
// If high retry rate:
// 1. Increase timeouts
// 2. Scale infrastructure
// 3. Optimize queries

// If zero retries:
// 1. May not need retries
// 2. Reduce retry count for faster failure
```

### 4. **Log Retry Events**
All retries are logged with:
- Request name
- Error type
- Error message
- Retry delay
- Current attempt/max attempts

### 5. **Don't Retry User Errors**
```csharp
// User errors (validation, auth) should not retry:
if (exception is ValidationException)
    return false;  // Don't retry

if (exception is UnauthorizedException)
    return false;  // Don't retry
```

## Retry Strategy Comparison

| Strategy | Description | Use Case |
|----------|-------------|----------|
| **No Retry** | Execute once | Payment, authorization |
| **Fixed Delay** | Wait same time each retry | Simple resilience |
| **Exponential Backoff** | Wait longer each retry | Network issues (CURRENT) |
| **Jittered Backoff** | Add randomness to delays | Prevent thundering herd |

## Pipeline Position

The recommended position for RetryBehavior is:

```
1. LoggingBehavior         ← Log all requests
2. CachingBehavior         ← Check cache
3. ValidationBehavior      ← Validate data
4. AuthorizationBehavior   ← Check permissions
5. RetryBehavior ← NEW     ← Handle transient failures
6. TimeoutBehavior         ← Enforce timeout
7. TransactionBehavior     ← Database transaction
```

**Why this position?**
- After validation (no point retrying invalid requests)
- After authorization (don't retry blocked requests)
- Before timeout (allow time for retries + original execution)
- Before transaction (ensure logic completes successfully)

## Testing Retry Behavior

### Test 1: Successful Retry After Transient Failure
```csharp
[TestMethod]
public async Task Handle_TransientFailureThenSuccess_Retries()
{
    // Arrange
    var behavior = new RetryBehavior<GetDataQuery, Data>(_mockLogger);
    int attemptCount = 0;
    
    // Act - First call fails, second succeeds
    Task<Data> Handler()
    {
        attemptCount++;
        if (attemptCount == 1)
            throw new IOException("Connection timeout");
        return Task.FromResult(new Data { Id = 1 });
    }
    
    var result = await behavior.Handle(new GetDataQuery(), Handler, CancellationToken.None);
    
    // Assert
    Assert.AreEqual(2, attemptCount);  // Retried once
    Assert.AreEqual(1, result.Id);
}
```

### Test 2: Immediate Failure on Permanent Error
```csharp
[TestMethod]
[ExpectedException(typeof(UnauthorizedException))]
public async Task Handle_PermanentError_NoRetry()
{
    // Arrange
    var behavior = new RetryBehavior<LoginCommand, User>(_mockLogger);
    
    // Act - Auth error should not retry
    await behavior.Handle(
        new LoginCommand(),
        async () => throw new UnauthorizedException(),
        CancellationToken.None);
}
```

### Test 3: Fails After Max Retries
```csharp
[TestMethod]
[ExpectedException(typeof(IOException))]
public async Task Handle_TransientFailureAlwaysOccurs_RetriesThenThrows()
{
    // Arrange
    var behavior = new RetryBehavior<GetDataQuery, Data>(_mockLogger);
    int attemptCount = 0;
    
    // Act - Always fails
    Task<Data> Handler()
    {
        attemptCount++;
        throw new IOException("Connection always fails");
    }
    
    await behavior.Handle(new GetDataQuery(), Handler, CancellationToken.None);
    
    // Assert
    // Should have tried original + 3 retries = 4 total attempts
    Assert.AreEqual(4, attemptCount);
}
```

## HTTP Status Codes

| Status | Meaning | When |
|--------|---------|------|
| **200** | Success | After retry succeeds |
| **503** | Service Unavailable | Transient failure after all retries |
| **500** | Server Error | Permanent error or max retries exceeded |

## Configuration Values Reference

```
Quick Queries (2-3 retries):
  - Get single entity: 3 retries
  - Get list data: 3 retries

Standard Commands (2 retries):
  - Authentication: 2 retries
  - Database updates: 2 retries

External Integration (3 retries):
  - SMS/Email: 3 retries
  - External APIs: 3 retries

Long Operations (1 retry):
  - Heavy computation: 1 retry
  - Simulations: 1 retry
  - Model training: 1 retry

No Retry (0 retries):
  - Payment processing: 0 retries
  - Authorization checks: 0 retries
  - Critical operations: 0 retries
```

## Backoff Calculation

With exponential backoff (default: 3 retries):

```
Retry 1: Wait 1 second   (2^0 = 1)
Retry 2: Wait 2 seconds  (2^1 = 2)
Retry 3: Wait 4 seconds  (2^2 = 4)

Total wait time: 7 seconds
Total possible time: Original execution + 7 seconds
```

## Related Files

- **RetryBehavior**: `NYCTaxiData.Application\Behaviors\RetryBehavior.cs`
- **Polly Package**: `Polly` v8.6.6
- **LoggingBehavior**: For logging retry events
- **TimeoutBehavior**: For enforcing total execution time

## Summary

The **RetryBehavior** provides:

✅ Automatic retry on transient failures  
✅ Exponential backoff to prevent thundering herd  
✅ Smart error detection (transient vs. permanent)  
✅ Configurable retries per operation  
✅ Comprehensive logging of all attempts  
✅ Polly integration for resilience  

It's essential for building resilient APIs that gracefully handle temporary network and database issues without cascading failures.
