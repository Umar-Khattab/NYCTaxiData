# Logging Behavior - Quick Reference

## What It Does

Logs every request that goes through MediatR pipeline:
- **Logs when it starts** - "Starting request execution: LoginCommand"
- **Tracks execution time** - Using Stopwatch
- **Logs when it completes** - "Request completed successfully: LoginCommand - Execution time: 145ms"
- **Logs exceptions** - With full error context if something fails

## Quick Example

```csharp
// Request goes through
var command = new LoginCommand("+1234567890", "password123");
await mediator.Send(command);

// Logs produced:
// [Information] Starting request execution: LoginCommand
// [Debug] Request details - Name: LoginCommand, Type: NYCTaxiData.Application.Auth.Commands.Login.LoginCommand
// [Information] Request completed successfully: LoginCommand - Execution time: 145ms
```

## Log Output Format

### Success
```
[Information] Starting request execution: LoginCommand
[Debug] Request details - Name: LoginCommand, Type: NYCTaxiData.Application.Auth.Commands.Login.LoginCommand
[Information] Request completed successfully: LoginCommand - Execution time: 145ms
```

### Failure
```
[Information] Starting request execution: UpdateThresholds
[Debug] Request details - Name: UpdateThresholds, Type: NYCTaxiData.Application.Features.Analytics.Commands.UpdateSystemThresholds.UpdateThresholdsCommand
[Error] Request execution failed: UpdateThresholds - Execution time: 87ms - Error: Database connection failed
```

## Integration (3 Steps)

### Step 1: Register in MediatR
```csharp
// In Program.cs or DependencyInjection.cs
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Register first (outermost layer)
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    
    // Other behaviors follow
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
```

### Step 2: Configure Log Levels (Optional)
```json
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "NYCTaxiData": "Debug"
    }
  }
}
```

### Step 3: That's it!
Logging is automatically configured when you create a WebApplication.

## Log Levels

| Level | When | Message |
|-------|------|---------|
| **Information** | Request starts/completes | "Starting request execution: ..." |
| **Debug** | Detailed info (dev only) | "Request details - Name: ..., Type: ..." |
| **Error** | Exceptions occur | "Request execution failed: ... - Error: ..." |

## Performance Impact

- **Minimal overhead**: Only adds Stopwatch start/stop
- **Fast logging**: All operations are synchronous
- **Non-blocking**: Doesn't slow down request execution

## Pipeline Position

```
REQUEST
  ↓
[1] LoggingBehavior ← FIRST (Logs everything)
  ↓
[2] ValidationBehavior (Validates data)
  ↓
[3] AuthorizationBehavior (Checks permissions)
  ↓
[4] TransactionBehavior (Manages DB)
  ↓
HANDLER (Executes logic)
```

## Finding Slow Requests

Use the execution time in logs:

```bash
# Find all operations taking more than 1 second
grep "Execution time: [0-9]\{4,\}ms" logs/log-*.txt

# Example output:
# [Information] Request completed successfully: GenerateReport - Execution time: 2456ms
# [Information] Request completed successfully: ExportData - Execution time: 1892ms
```

## Common Use Cases

### Track All User Actions
```
[Information] Starting request execution: LoginCommand
[Information] Request completed successfully: LoginCommand - Execution time: 145ms
[Information] Starting request execution: GetProfileQuery
[Information] Request completed successfully: GetProfileQuery - Execution time: 28ms
[Information] Starting request execution: UpdateThresholdsCommand
[Information] Request completed successfully: UpdateThresholdsCommand - Execution time: 567ms
```

### Identify Failed Operations
```bash
# Find all failed requests
grep "\[Error\]" logs/log-*.txt

# Example:
[Error] Request execution failed: UpdateThresholds - Execution time: 87ms - Error: Database connection failed
[Error] Request execution failed: ProcessPayment - Execution time: 234ms - Error: Payment gateway timeout
```

### Performance Analysis
```bash
# Get average response time for a command
grep "UpdateThresholds" logs/log-*.txt | \
  grep "Execution time" | \
  sed 's/.*Execution time: \([0-9]*\)ms.*/\1/' | \
  awk '{sum+=$1; count++} END {print "Average: " sum/count "ms"}'
```

## Advanced: Structured Logging with Serilog

### Install
```bash
dotnet add package Serilog.AspNetCore
```

### Configure in Program.cs
```csharp
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));
```

### Benefits
- Better structured data for queries
- Log correlation IDs
- Enriched context information
- Easier integration with log aggregation tools

## What Gets Logged

✅ Request name (e.g., "LoginCommand")  
✅ Request type (full namespace)  
✅ Execution time in milliseconds  
✅ Success or failure status  
✅ Full exception details on errors  

❌ NOT logged: Request parameters (for security)  
❌ NOT logged: Response data (too much noise)  

## Tips

1. **Order matters**: LoggingBehavior MUST be first
2. **Use appropriate levels**: Debug for dev, Info for prod
3. **Monitor performance**: Track slow requests
4. **Secure sensitive data**: Don't log passwords/tokens
5. **Centralize logs**: Use log aggregation in production

## Related Documentation

- See `LOGGING_README.md` for complete guide
- See `INDEX.md` for all behaviors overview
- See `AUTHORIZATION_VS_VALIDATION.md` for pipeline comparison

## Build Status

✅ Build Successful - Ready to use!
