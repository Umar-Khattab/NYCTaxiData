# Retry Behavior - Quick Reference

## What It Does

Automatically retries failed requests on transient errors:
- **Database connection failures** → Retry
- **Network timeouts** → Retry
- **Temporary service issues** → Retry
- **Validation errors** → NO retry
- **Authorization errors** → NO retry

## Quick Example

```csharp
// First attempt fails with connection error
// Waits 1 second, retries (Attempt 2)
// Still fails, waits 2 seconds, retries (Attempt 3)
// Succeeds on Attempt 3
var result = await mediator.Send(query);
// Total time: 3 seconds (original + retries + waits)
```

## Default Retry Counts

```
QUERIES (Fast, Idempotent):
  GetProfileQuery ....................... 3 retries
  GetActiveFleetQuery .................. 3 retries

COMMANDS (Database):
  LoginCommand ......................... 2 retries
  RegisterCommand ..................... 2 retries
  SyncOfflineTripsCommand ............. 3 retries

EXTERNAL INTEGRATIONS (Network):
  SendOtpCommand ....................... 3 retries
  ProcessVoiceAssistantQuery .......... 2 retries

LONG OPERATIONS (Already slow):
  RunOperationalSimulationCommand ... 1 retry
  TriggerModelRetrainingCommand ..... 1 retry

UNKNOWN ................................ 3 retries (default)
```

## Integration (1 Line)

```csharp
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
```

That's it! Retries work automatically.

## Log Output

### Successful (After Retry)
```
[Debug] Request will be executed with retry policy (max 3 retries)
[Warning] Request failed with SqlException. Retrying in 1000ms (Attempt 1/3)
[Information] Request completed successfully - Execution time: 1342ms
```

### Failed (After All Retries)
```
[Debug] Request will be executed with retry policy (max 3 retries)
[Warning] Request failed. Retrying in 1000ms (Attempt 1/3)
[Warning] Request failed. Retrying in 2000ms (Attempt 2/3)
[Warning] Request failed. Retrying in 4000ms (Attempt 3/3)
[Error] Request failed after 3 retries. Final error: Connection permanently closed
```

## Exponential Backoff

```
Attempt 1: Immediate
Attempt 2: Wait 1 second
Attempt 3: Wait 2 seconds
Attempt 4: Wait 4 seconds

Total: 7 seconds of waiting for 4 attempts
```

## Transient Errors (Retried)

✅ Database connection failures  
✅ Network timeouts  
✅ Service temporarily unavailable  
✅ IOException  
✅ SQL/PostgreSQL connection errors  

## Permanent Errors (NOT Retried)

❌ ValidationException  
❌ UnauthorizedException  
❌ OperationCanceledException  
❌ Business logic errors  

## Pipeline Position

```
REQUEST
  ↓
LoggingBehavior .............. Log start
  ↓
CachingBehavior .............. Check cache
  ↓
ValidationBehavior ........... Validate input
  ↓
AuthorizationBehavior ........ Check permissions
  ↓
RetryBehavior ← HERE ........ Handle transient failures
  ↓
TimeoutBehavior .............. Enforce timeout
  ↓
TransactionBehavior .......... Begin transaction
  ↓
HANDLER
```

## Best Practices

✅ **DO**: 
- Retry queries (idempotent)
- Retry network operations
- Use low retry counts for commands
- Monitor retry rates

❌ **DON'T**:
- Retry payment operations
- Retry authorization
- Retry validation errors
- Use high retry counts everywhere

## Customizing Retries

Edit `GetRetryCountForRequest` method:

```csharp
return requestName switch
{
    "MyQuery" => 5,           // Increase retries
    "CriticalOp" => 0,        // No retries
    _ => DefaultRetryCount
};
```

## Testing

```csharp
// Test 1: Success after retry
attemptCount = 0;
Handler() => attemptCount++ == 1 ? throw IOException() : success;
// Result: Retries once, then succeeds

// Test 2: No retry on permanent error
Handler() => throw UnauthorizedException();
// Result: Fails immediately, no retry
```

## Common Scenarios

### Scenario 1: Database Temporarily Down
```
Attempt 1: Connection refused (RETRY)
Wait 1 second
Attempt 2: Connection refused (RETRY)
Wait 2 seconds
Attempt 3: SUCCESS ✓
```

### Scenario 2: Network Timeout
```
Attempt 1: Network timeout (RETRY)
Wait 1 second
Attempt 2: SUCCESS ✓
```

### Scenario 3: Invalid Credentials
```
Attempt 1: Invalid credentials (DON'T RETRY)
Fail immediately ✗
```

## Performance Tips

1. **Query timeouts**: 30 seconds + retry time
2. **Low retry commands**: Faster failure detection
3. **External APIs**: Higher retry count
4. **Critical ops**: Zero retries

## HTTP Status Codes

- **200 OK** - Request succeeded (after retry or first attempt)
- **503 Service Unavailable** - Transient failure after all retries
- **500 Server Error** - Permanent error

## Related Documentation

- See `RETRY_README.md` for complete guide
- See `INDEX.md` for all behaviors
- See `COMPLETE_IMPLEMENTATION.md` for full overview

## Build Status

✅ Build Successful - Ready to use!

## Key Takeaways

✅ Automatic retry on transient failures  
✅ Exponential backoff (1s, 2s, 4s, ...)  
✅ Smart error detection  
✅ Configurable per operation  
✅ Comprehensive logging  

## Dependencies

- ✅ **Polly** v8.6.6 - Already installed
- ✅ **MediatR** - Already installed
- ✅ **Logging** - Uses ILogger
