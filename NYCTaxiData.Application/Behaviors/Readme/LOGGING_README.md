# Logging Behavior Implementation Guide

## Overview
The `LoggingBehavior` is a MediatR pipeline behavior that logs all requests, responses, and execution times. It provides visibility into command/query execution for monitoring, debugging, and auditing purposes.

## Components

### LoggingBehavior
Located in: `NYCTaxiData.Application\Behaviors\LoggingBehavior.cs`

This behavior logs information about every request processed through the MediatR pipeline.

**Features:**
- Logs request start and completion
- Tracks execution time using `Stopwatch`
- Logs successful completions with timing
- Logs exceptions with full context
- Uses appropriate log levels (Information, Debug, Error)

## How It Works

### Log Levels Used

| Level | When Used | Example |
|-------|-----------|---------|
| **Information** | Request starts and completes | "Starting request execution: LoginCommand" |
| **Debug** | Detailed request info | "Request details - Name: LoginCommand, Type: ..." |
| **Error** | Exception occurs during execution | "Request execution failed: UpdateThresholds - Error: ..." |

### Execution Flow

```
1. Request arrives
    ↓
2. LoggingBehavior receives it
    ↓
3. Log: "Starting request execution: {RequestName}"
    ↓
4. Log Debug: "Request details - Name: ..., Type: ..."
    ↓
5. Start Stopwatch
    ↓
6. Call next behavior
    ↓
7. Response returns
    ↓
8. Stop Stopwatch
    ↓
9. Log: "Request completed successfully: {RequestName} - {ExecutionTime}ms"
    ↓
10. Return response
```

## Log Output Examples

### Successful Request
```
[Information] Starting request execution: LoginCommand
[Debug] Request details - Name: LoginCommand, Type: NYCTaxiData.Application.Auth.Commands.Login.LoginCommand
[Information] Request completed successfully: LoginCommand - Execution time: 145ms
```

### Failed Request
```
[Information] Starting request execution: UpdateThresholdsCommand
[Debug] Request details - Name: UpdateThresholdsCommand, Type: NYCTaxiData.Application.Features.Analytics.Commands.UpdateSystemThresholds.UpdateThresholdsCommand
[Error] Request execution failed: UpdateThresholdsCommand - Execution time: 87ms - Error: Database connection failed
```

## Integration Steps

### Step 1: Register in MediatR Configuration
```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Register LoggingBehavior as the first behavior (outermost layer)
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    
    // Other behaviors follow
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
});
```

### Step 2: Configure Logging in Program.cs (Optional - may already be configured)
```csharp
// Logging is automatically configured by WebApplication.CreateBuilder()
// But you can customize it:

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// For structured logging, add Serilog or similar:
// builder.Host.UseSerilog((context, configuration) =>
//     configuration.ReadFrom.Configuration(context.Configuration));
```

### Step 3: Configure Log Levels in appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "NYCTaxiData": "Debug"
    }
  }
}
```

## Understanding Log Levels

### Information Level
Best for tracking application flow and high-level events.

```csharp
// What gets logged
_logger.LogInformation(
    "Starting request execution: {RequestName}",
    requestName);
// Output: "Starting request execution: LoginCommand"
```

### Debug Level
Use for detailed diagnostic information useful during development.

```csharp
// What gets logged
_logger.LogDebug(
    "Request details - Name: {RequestName}, Type: {RequestType}",
    requestName,
    typeof(TRequest).FullName);
// Output: "Request details - Name: LoginCommand, Type: NYCTaxiData.Application.Auth.Commands.Login.LoginCommand"
```

### Error Level
Use when exceptions occur or something goes wrong.

```csharp
// What gets logged
_logger.LogError(
    ex,
    "Request execution failed: {RequestName} - Execution time: {ExecutionTime}ms - Error: {ErrorMessage}",
    requestName,
    stopwatch.ElapsedMilliseconds,
    ex.Message);
// Output: "Request execution failed: UpdateThresholds - Execution time: 87ms - Error: Database connection failed"
```

## Performance Monitoring

The behavior tracks execution time using `Stopwatch`:

```csharp
var stopwatch = Stopwatch.StartNew();

// ... request execution ...

stopwatch.Stop();
var elapsedMs = stopwatch.ElapsedMilliseconds;
```

### Log Output with Timing
```
[Information] Request completed successfully: LoginCommand - Execution time: 145ms
[Information] Request completed successfully: GetProfileQuery - Execution time: 23ms
[Information] Request completed successfully: UpdateThresholdsCommand - Execution time: 567ms
```

### Identifying Slow Requests
By monitoring the logs, you can identify requests that exceed expected execution times:

```csharp
// In appsettings.json, you could add custom alerts for slow queries
// Or use structured logging to query for long-running operations
```

## Best Practices

### 1. **Pipeline Order is Critical**
LoggingBehavior should be registered **first** (outermost layer):

```csharp
// ✅ CORRECT - LoggingBehavior first
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));      // Outermost
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));    // Middle
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>)); // Middle
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));   // Innermost

// ❌ INCORRECT - LoggingBehavior in wrong position
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

### 2. **Use Structured Logging**
For better querying and analysis, use structured logging (Serilog, NLog):

```csharp
// Using Serilog (example)
_logger.LogInformation(
    "Request completed {@Request} in {ExecutionTime}ms with result {@Result}",
    request,
    stopwatch.ElapsedMilliseconds,
    response);
```

### 3. **Monitor Log Levels**
Configure appropriate log levels for different environments:

```json
// Development - verbose logging
"LogLevel": {
  "Default": "Debug",
  "Microsoft": "Debug"
}

// Staging - balanced logging
"LogLevel": {
  "Default": "Information",
  "Microsoft": "Warning"
}

// Production - minimal logging
"LogLevel": {
  "Default": "Information",
  "Microsoft": "Warning",
  "NYCTaxiData": "Information"
}
```

### 4. **Watch Out for Sensitive Data**
Be careful not to log sensitive information:

```csharp
// ❌ DON'T log passwords or tokens
_logger.LogInformation("User login with password: {Password}", request.Password);

// ✅ DO log only necessary info
_logger.LogInformation("User login attempt for phone: {PhoneNumber}", request.PhoneNumber);
```

### 5. **Use Exception Information**
When logging errors, include the full exception:

```csharp
// ✅ GOOD - includes exception details
_logger.LogError(ex, "Request failed: {RequestName}", requestName);

// ❌ POOR - loses exception context
_logger.LogError("Request failed: {RequestName}", requestName);
```

## Structured Logging with Serilog (Optional)

For advanced logging scenarios, integrate Serilog:

### Install Serilog
```bash
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### Configure in Program.cs
```csharp
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File(
            "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .Enrich.WithProperty("Application", "NYCTaxiData")
        .ReadFrom.Configuration(context.Configuration));
```

### appsettings.json Configuration
```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ],
    "Properties": {
      "Application": "NYCTaxiData"
    }
  }
}
```

## Log Aggregation & Analysis

### Viewing Logs
```bash
# On Linux/Mac
tail -f logs/log-2024-01-15.txt

# On Windows
Get-Content logs/log-2024-01-15.txt -Tail 50 -Wait
```

### Searching for Specific Requests
```bash
# Find all login attempts
grep "LoginCommand" logs/log-*.txt

# Find all failed requests
grep "ERROR" logs/log-*.txt

# Find slow queries (execution time > 1000ms)
grep -E "Execution time: [0-9]{4,}ms" logs/log-*.txt
```

### Query Performance
```bash
# Get average execution time for a request
grep "UpdateThresholdsCommand" logs/log-*.txt | \
  grep "Execution time" | \
  sed 's/.*Execution time: \([0-9]*\)ms.*/\1/' | \
  awk '{sum+=$1; count++} END {print sum/count}'
```

## Pipeline Order (Complete)

The recommended order for all behaviors is:

```
Layer 1: LoggingBehavior ← OUTERMOST (NEW)
  (Logs all requests and times execution)
    ↓
Layer 2: MetricsAndPerformanceBehavior
  (Tracks performance metrics)
    ↓
Layer 3: CachingBehavior
  (Checks and returns cached data)
    ↓
Layer 4: ValidationBehavior
  (Validates input data)
    ↓
Layer 5: AuthorizationBehavior
  (Checks user permissions)
    ↓
Layer 6: IdempotencyBehavior
  (Prevents duplicate execution)
    ↓
Layer 7: TransactionBehavior ← INNERMOST
  (Manages database transactions)
    ↓
HANDLER (Executes business logic)
```

## Testing

```csharp
[TestClass]
public class LoggingBehaviorTests
{
    private Mock<ILogger<LoggingBehavior<TestCommand, Unit>>> _mockLogger;
    private LoggingBehavior<TestCommand, Unit> _behavior;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<LoggingBehavior<TestCommand, Unit>>>();
        _behavior = new LoggingBehavior<TestCommand, Unit>(_mockLogger.Object);
    }

    [TestMethod]
    public async Task Handle_OnSuccess_LogsInformationMessages()
    {
        // Arrange
        var command = new TestCommand();
        Task<Unit> next() => Task.FromResult(Unit.Value);

        // Act
        await _behavior.Handle(command, next, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2)); // Start and completion messages
    }

    [TestMethod]
    public async Task Handle_OnException_LogsErrorMessage()
    {
        // Arrange
        var command = new TestCommand();
        var exception = new InvalidOperationException("Test error");
        Task<Unit> next() => throw exception;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => _behavior.Handle(command, next, CancellationToken.None));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

public class TestCommand : IRequest<Unit>
{
}
```

## Troubleshooting

### Issue: Logs not appearing
**Solution**: 
1. Check log level configuration in appsettings.json
2. Ensure logger provider is configured (Console, File, etc.)
3. Verify behavior is registered in MediatR configuration

### Issue: Too many debug logs
**Solution**:
1. Increase minimum log level to "Information" in appsettings.json
2. Filter out verbose namespaces

### Issue: Performance impact from logging
**Solution**:
1. Use appropriate log levels for environment
2. Log less frequently in production
3. Consider async file logging with Serilog

### Issue: Large log files
**Solution**:
1. Enable rolling file policies with Serilog
2. Implement log retention policies
3. Archive old logs

## Related Files

- **LoggingBehavior**: `NYCTaxiData.Application\Behaviors\LoggingBehavior.cs`
- **Global Exception Handler**: `NYCTaxiData.API\MiddleWares\GlobalExceptionHandler.cs`
- **Configuration**: `appsettings.json`
- **Program.cs**: Logging configuration
- **Other Behaviors**: See `AUTHORIZATION_README.md`, `VALIDATION_README.md`

## Summary

The **LoggingBehavior** provides:

✅ Request execution tracking  
✅ Performance monitoring with timing  
✅ Exception logging with context  
✅ Structured logging support  
✅ Production-ready monitoring  

It's the **outermost layer** of the MediatR pipeline, ensuring all requests are logged regardless of success or failure.
