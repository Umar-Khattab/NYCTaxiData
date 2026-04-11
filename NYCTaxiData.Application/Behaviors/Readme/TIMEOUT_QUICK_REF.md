# Timeout Behavior - Quick Reference

## What It Does

Automatically cancels requests that take too long:
- **Queries**: 10-25 seconds
- **Commands**: 15-30 seconds  
- **AI Operations**: 60-120 seconds
- **Default**: 30 seconds

## Quick Example

```csharp
// This takes 100ms - completes successfully ✓
var query = new GetProfileQuery();
await mediator.Send(query);
// Response: UserProfile

// This takes 120 seconds - times out ✗
var command = new TriggerRetrainingCommand();
await mediator.Send(command);
// Throws: OperationCanceledException after 120 seconds
```

## Default Timeouts

```
QUERIES (Fast):
  GetProfileQuery ..................... 10 seconds
  GetActiveFleetQuery ................. 15 seconds
  GetAllZonesQuery .................... 15 seconds
  GetTopLevelKpisQuery ................ 20 seconds

COMMANDS (Standard):
  LoginCommand ........................ 15 seconds
  RegisterCommand ..................... 20 seconds
  UpdateSystemThresholdsCommand ....... 30 seconds
  StartTripCommand .................... 20 seconds
  EndTripCommand ...................... 25 seconds

AI OPERATIONS (Long):
  RunOperationalSimulationCommand ..... 60 seconds
  RunStrategicSimulationCommand ....... 90 seconds
  TriggerModelRetrainingCommand ....... 120 seconds

Unknown Request Type .................. 30 seconds (default)
```

## Integration (1 Step)

### Register in MediatR
```csharp
// In Program.cs or DependencyInjection.cs
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TimeoutBehavior<,>));
});
```

That's it! Timeouts work automatically.

## Log Output

### Successful (within timeout)
```
[Debug] Request GetProfileQuery will be executed with timeout of 10 seconds
[Information] Request completed successfully: GetProfileQuery - Execution time: 245ms
```

### Timeout Exceeded
```
[Debug] Request RunSimulation will be executed with timeout of 60 seconds
[Error] Request RunSimulation exceeded timeout of 60 seconds
System.OperationCanceledException: A task was canceled.
```

## Error Response

```json
HTTP/1.1 408 Request Timeout
{
    "message": "Request exceeded timeout limit",
    "statusCode": 408
}
```

## How to Customize Timeouts

Edit `TimeoutBehavior.cs` GetTimeoutForRequest method:

```csharp
private int GetTimeoutForRequest(string requestName)
{
    return requestName switch
    {
        "MySlowQuery" => 60,          // Increase timeout
        "MyFastCommand" => 5,         // Decrease timeout
        "NewOperation" => 45,         // Add new operation
        _ => DefaultTimeoutSeconds
    };
}
```

## Timeout Strategy

Uses Polly's **Optimistic** timeout strategy:
- ✅ Graceful cancellation via CancellationToken
- ✅ Allows resources to clean up
- ✅ Safer for database operations
- ✅ Prevents resource leaks

## Pipeline Position

```
REQUEST
  ↓
[1] LoggingBehavior
[2] CachingBehavior
[3] ValidationBehavior
[4] AuthorizationBehavior
[5] TimeoutBehavior ← HERE
[6] TransactionBehavior
  ↓
HANDLER
```

## Best Practices

✅ **DO**: 
- Set realistic timeouts
- Different timeouts for queries vs. commands
- Give AI/heavy operations more time
- Monitor logs for frequent timeouts

❌ **DON'T**:
- Set timeouts too short (will always timeout)
- Set timeouts too long (defeats purpose)
- Use same timeout for all operations
- Ignore timeout errors in production

## Common Timeouts

```
Read Single Record    →  10 seconds
Read List/Report      →  15 seconds
Simple Update         →  15 seconds
Complex Processing    →  30 seconds
Report Generation     →  45 seconds
Simulation            →  60 seconds
Model Training        →  120 seconds
```

## Testing

```csharp
// Test success
var command = new FastCommand();
await behavior.Handle(command, async () => Unit.Value, CancellationToken.None);
// ✓ Returns successfully

// Test timeout
var command = new SlowCommand();
await behavior.Handle(command, async () => { 
    await Task.Delay(999999); 
    return Unit.Value; 
}, CancellationToken.None);
// ✗ Throws OperationCanceledException
```

## Related Documentation

- See `TIMEOUT_README.md` for complete guide
- See `INDEX.md` for all behaviors
- See `COMPLETE_IMPLEMENTATION.md` for full overview

## Build Status

✅ Build Successful - Ready to use!

## HTTP Status Codes

- **408 Request Timeout** - Request exceeded timeout
- **504 Gateway Timeout** - External dependency timed out

## Performance Tips

1. Monitor actual execution times
2. Adjust timeouts to 1.5x-2x of typical execution time
3. Watch for slow database queries
4. Optimize bottlenecks
5. Scale infrastructure if needed

## Dependencies

- ✅ **Polly** v8.6.6 - Already installed
- ✅ **MediatR** - Already installed
- ✅ **Logging** - Uses ILogger

## Key Files

- `TimeoutBehavior.cs` - Implementation
- `TIMEOUT_README.md` - Complete guide
- `GlobalExceptionHandler.cs` - Error handling
