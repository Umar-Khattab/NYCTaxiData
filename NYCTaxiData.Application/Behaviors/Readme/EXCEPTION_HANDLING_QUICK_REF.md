# Exception Handling Behavior - Quick Reference

## What It Does

Catches all exceptions in the pipeline and logs them appropriately:
- **Catches exceptions** - From any behavior or handler
- **Categorizes errors** - Different severity for different exception types
- **Logs context** - Includes exception details for debugging
- **Re-throws** - Lets middleware handle HTTP response
- **Provides consistency** - Same logging for all exceptions

## Quick Example

```csharp
// Exception thrown in handler
throw new NotFoundException("Order", "order-123");

// ExceptionHandlingBehavior catches it
[Warning] Resource not found for request GetOrderQuery. Resource: Order

// Middleware converts to 404 response
```

## When Applied

```
All Requests → Exception Handling Applied ✓
├─ If exception occurs
├─ Logs appropriately
└─ Re-throws for middleware
```

## Integration (2 Steps)

### Step 1: Register Last in Pipeline
```csharp
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>)); // ← LAST!
```

### Step 2: Register Middleware
```csharp
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

That's it! Exception handling works automatically.

## Exception Types

```
WARNING Severity:
  ✅ ValidationException        - Invalid input
  ✅ UnauthorizedException      - Access denied
  ✅ NotFoundException           - Resource missing
  ✅ ConflictException           - Duplicate/conflict
  ✅ OperationCanceledException - Cancelled

ERROR Severity:
  ❌ TimeoutException            - Operation timed out
  ❌ InvalidOperationException   - Invalid state
  ❌ ArgumentException           - Invalid argument
  ❌ Other Exception             - Unhandled errors
```

## Log Output Examples

### Validation Error
```
[Warning] Validation exception for request CreateOrderCommand.
          Errors: PhoneNumber - Invalid format; Password - Too short
```

### Unauthorized
```
[Warning] Unauthorized access attempt for request UpdateOrderCommand.
          User: user-123
```

### Not Found
```
[Warning] Resource not found for request GetOrderQuery.
          Resource: Order
```

### Conflict
```
[Warning] Conflict for request CreateUserCommand.
          Details: User with email already exists
```

### Timeout
```
[Error] Timeout occurred for request RunSimulationCommand
```

### Unhandled
```
[Error] Unhandled exception for request CreateOrderCommand.
        Exception type: NullReferenceException.
        Message: Object reference not set to an instance of an object
```

## Pipeline Position

```
REQUEST
  ↓
[1] MetricsBehavior ........... Collect metrics
[2] PerformanceBehavior ....... Monitor performance
[3] LoggingBehavior .......... Log request
[4] CachingBehavior .......... Return cached
[5] ValidationBehavior ....... Validate
[6] AuthorizationBehavior .... Check permissions
[7] IdempotencyBehavior ...... Prevent duplicates
[8] RetryBehavior ........... Retry failures
[9] TimeoutBehavior ......... Enforce timeout
[10] ExceptionHandlingBehavior ← HERE (LAST!)
[11] TransactionBehavior ..... Manage transaction
  ↓
HANDLER
```

## Using Custom Exceptions

### NotFoundException
```csharp
var order = await _db.Orders.FindAsync(orderId)
    ?? throw new NotFoundException("Order", orderId);
```

### UnauthorizedException
```csharp
if (!canAccess)
    throw new UnauthorizedException("Access denied", "Insufficient role", userId);
```

### ConflictException
```csharp
if (userExists)
    throw new ConflictException("User already exists", "User", email);
```

## HTTP Status Codes

```
ValidationException         → 400 Bad Request
UnauthorizedException       → 403 Forbidden
NotFoundException            → 404 Not Found
ConflictException            → 409 Conflict
OperationCanceledException   → 408 Request Timeout
TimeoutException             → 408 Request Timeout
Other Exception              → 500 Internal Server Error
```

## Best Practices

✅ **DO**:
- Use specific exception types
- Provide context/details
- Let exceptions propagate
- Use middleware for HTTP response

❌ **DON'T**:
- Catch and ignore exceptions
- Use generic Exception type
- Return null/default on error
- Swallow error messages

## Related Documentation

- See `EXCEPTION_HANDLING_README.md` for complete guide
- See `INDEX.md` for all behaviors
- See `COMPLETE_IMPLEMENTATION.md` for full overview

## Build Status

✅ Build Successful - Ready to use!

## Key Takeaways

✅ Centralized exception handling  
✅ Type-based categorization  
✅ Appropriate severity logging  
✅ Exception context preserved  
✅ Middleware integration ready  

## Dependencies

- ✅ **MediatR** - Already installed
- ✅ **Logging** - Uses ILogger
- ✅ **Custom Exceptions** - Defined in behavior
