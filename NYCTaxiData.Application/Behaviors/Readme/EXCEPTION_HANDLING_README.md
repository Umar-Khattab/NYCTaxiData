# Exception Handling Behavior Implementation Guide

## Overview
The `ExceptionHandlingBehavior` is a MediatR pipeline behavior that provides centralized exception handling across the request pipeline. It catches exceptions, logs them with appropriate severity levels, categorizes them by type, and re-throws them for middleware to handle. This behavior works in conjunction with global exception middleware for comprehensive error handling.

## Components

### ExceptionHandlingBehavior
Located in: `NYCTaxiData.Application\Behaviors\ExceptionHandlingBehavior.cs`

This behavior catches and logs all exceptions in the pipeline.

**Features:**
- Centralized exception handling
- Type-based exception logging
- Appropriate severity levels per exception type
- Exception context preservation
- Re-throws for middleware handling
- Production-ready implementation

### Custom Exception Classes

**NotFoundException**
- Thrown when a resource cannot be found
- Properties: ResourceType, ResourceId

**UnauthorizedException**
- Thrown when access is denied
- Properties: UserId, Reason

**ConflictException**
- Thrown for duplicate or conflicting resources
- Properties: ResourceType, ResourceId

**ValidationException** (existing)
- Thrown when input validation fails
- Properties: Errors (dictionary)

## How It Works

### Request Processing Flow

```
REQUEST
    ↓
Try to execute handler
    ↓
On success → Return response
    ↓
On exception:
  ├─ ValidationException
  │  └─ Log WARNING with validation errors
  │
  ├─ UnauthorizedException
  │  └─ Log WARNING with user ID
  │
  ├─ NotFoundException
  │  └─ Log WARNING with resource type/ID
  │
  ├─ ConflictException
  │  └─ Log WARNING with conflict details
  │
  ├─ OperationCanceledException
  │  └─ Log WARNING (operation cancelled)
  │
  ├─ TimeoutException
  │  └─ Log ERROR (timeout)
  │
  ├─ InvalidOperationException
  │  └─ Log ERROR (invalid operation)
  │
  ├─ ArgumentException
  │  └─ Log ERROR (invalid argument)
  │
  └─ Other Exception
     └─ Log ERROR (unhandled exception)
    ↓
Re-throw exception for middleware
```

### Exception Severity Levels

```
WARNING (Expected errors):
  ✅ ValidationException     - Client error (invalid input)
  ✅ UnauthorizedException   - Client error (not authorized)
  ✅ NotFoundException        - Client error (resource missing)
  ✅ ConflictException        - Client error (duplicate/conflict)
  ✅ OperationCanceledException - Operation cancelled

ERROR (Unexpected errors):
  ❌ TimeoutException         - Operation timed out
  ❌ InvalidOperationException - Invalid state
  ❌ ArgumentException        - Invalid argument
  ❌ Other Exception          - Unhandled errors
```

## Integration Steps

### Step 1: Register in MediatR Configuration (Last)
```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>)); // ← NEW (LAST)
    // ... other behaviors
});
```

### Step 2: Register Global Exception Middleware
```csharp
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

### Step 3: That's It!
All exceptions are caught, logged, and handled.

## Log Output Examples

### Validation Error (WARNING)
```
[Warning] Validation exception for request CreateOrderCommand. Errors: PhoneNumber - Invalid format; Password - Too short
```

### Unauthorized Access (WARNING)
```
[Warning] Unauthorized access attempt for request UpdateOrderCommand. User: user-123
```

### Resource Not Found (WARNING)
```
[Warning] Resource not found for request GetOrderQuery. Resource: Order
```

### Conflict (WARNING)
```
[Warning] Conflict for request CreateUserCommand. Details: User with email already exists
```

### Timeout (ERROR)
```
[Error] Timeout occurred for request RunSimulationCommand
```

### Unhandled Exception (ERROR)
```
[Error] Unhandled exception for request CreateOrderCommand. Exception type: NullReferenceException. Message: Object reference not set to an instance of an object
```

## Best Practices

### 1. **Use Appropriate Exception Types**
```csharp
// ✅ GOOD - Specific exception types
if (user == null)
    throw new NotFoundException("User", userId);

if (!userCanAccess)
    throw new UnauthorizedException("Access denied", "Insufficient permissions", currentUser.Id);

if (userExists)
    throw new ConflictException("User already exists", "User", email);

// ❌ BAD - Generic exceptions
if (user == null)
    throw new Exception("User not found");
```

### 2. **Provide Context**
```csharp
// ✅ GOOD - Include relevant information
throw new NotFoundException("Order", orderId);
throw new UnauthorizedException("Cannot update order", "Insufficient role", userId);
throw new ConflictException($"Order {orderId} already processed", "Order", orderId);

// ❌ BAD - Generic messages
throw new NotFoundException("", "");
throw new UnauthorizedException("Error", "Error", "");
```

### 3. **Leverage Exception Properties**
```csharp
// Custom exception handler can use properties
try
{
    await mediator.Send(command);
}
catch (NotFoundException ex)
{
    Log($"Resource {ex.ResourceType} {ex.ResourceId} not found");
}
catch (UnauthorizedException ex)
{
    Log($"User {ex.UserId} unauthorized: {ex.Reason}");
}
```

### 4. **Don't Catch and Ignore**
```csharp
// ✅ GOOD - Let exception propagate for handling
public async Task<Order> GetOrder(string orderId)
{
    var order = await _db.Orders.FindAsync(orderId);
    if (order == null)
        throw new NotFoundException("Order", orderId);
    return order;
}

// ❌ BAD - Swallowing exceptions
public async Task<Order?> GetOrder(string orderId)
{
    try
    {
        var order = await _db.Orders.FindAsync(orderId);
        return order;
    }
    catch
    {
        return null;  // Hides errors!
    }
}
```

## Exception Hierarchy and HTTP Status Codes

```
ValidationException         → 400 Bad Request
UnauthorizedException       → 403 Forbidden
NotFoundException            → 404 Not Found
ConflictException            → 409 Conflict
OperationCanceledException   → 408 Request Timeout
TimeoutException             → 408 Request Timeout
InvalidOperationException    → 500 Internal Server Error
ArgumentException            → 400 Bad Request
Other Exception              → 500 Internal Server Error
```

## Pipeline Position

The recommended position for ExceptionHandlingBehavior is:

```
1. MetricsBehavior ............ Collect metrics (before exception)
2. PerformanceBehavior ........ Monitor performance (before exception)
3. LoggingBehavior ........... Log request (before exception)
4. CachingBehavior ........... Return cached (before exception)
5. ValidationBehavior ........ Validate (before exception)
6. AuthorizationBehavior ..... Check auth (before exception)
7. IdempotencyBehavior ....... Prevent duplicates (before exception)
8. RetryBehavior ............ Retry failures (before exception)
9. TimeoutBehavior .......... Enforce timeout (before exception)
10. ExceptionHandlingBehavior ← HERE (LAST/OUTERMOST)
11. TransactionBehavior ....... Manage transaction (innermost)
```

**Why outermost?**
- Catches exceptions from ALL other behaviors
- All exception info available for logging
- Prevents unhandled exceptions from escaping
- Works with middleware for complete error handling

## Using Custom Exceptions

### Example: Get Order Handler
```csharp
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken ct)
    {
        // Will throw NotFoundException if not found
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId)
            ?? throw new NotFoundException("Order", request.OrderId);

        return _mapper.Map<OrderDto>(order);
    }
}
```

### Example: Validation Error
```csharp
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResultDto>
{
    public async Task<OrderResultDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // Validation errors are caught by ValidationBehavior
        // ExceptionHandlingBehavior logs them as warnings
        
        var order = new Order { /* ... */ };
        // ... create order ...
        return result;
    }
}
```

### Example: Unauthorized Access
```csharp
[Authorize(UserRole.Manager)]
public record UpdateOrderCommand(string OrderId, ...) : IRequest<Unit>;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Unit>
{
    private readonly ICurrentUserService _currentUser;

    public async Task<Unit> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        // Authorization checked by AuthorizationBehavior
        // ExceptionHandlingBehavior logs UnauthorizedException
        
        var order = await _db.Orders.FindAsync(request.OrderId)
            ?? throw new NotFoundException("Order", request.OrderId);

        order.UpdateStatus("Processed");
        await _db.SaveChangesAsync();
        return Unit.Value;
    }
}
```

## Testing Exception Handling

### Test 1: ValidationException Logging
```csharp
[TestMethod]
public async Task Handle_ValidationException_LogsWarning()
{
    var command = new CreateOrderCommand { PhoneNumber = "invalid" };
    var behavior = new ExceptionHandlingBehavior<CreateOrderCommand, Unit>(_logger);
    
    await Assert.ThrowsExceptionAsync<ValidationException>(
        () => behavior.Handle(command, 
            async () => throw new ValidationException(), 
            CancellationToken.None));
    
    _loggerMock.Verify(l => l.Log(LogLevel.Warning, ...), Times.Once);
}
```

### Test 2: NotFoundException Logging
```csharp
[TestMethod]
public async Task Handle_NotFoundException_LogsWarning()
{
    var query = new GetOrderQuery { OrderId = "invalid" };
    var behavior = new ExceptionHandlingBehavior<GetOrderQuery, OrderDto>(_logger);
    
    await Assert.ThrowsExceptionAsync<NotFoundException>(
        () => behavior.Handle(query, 
            async () => throw new NotFoundException("Order", "invalid"), 
            CancellationToken.None));
    
    _loggerMock.Verify(l => l.Log(LogLevel.Warning, ...), Times.Once);
}
```

### Test 3: Unhandled Exception Logging
```csharp
[TestMethod]
public async Task Handle_UnhandledException_LogsError()
{
    var command = new CreateOrderCommand { /* ... */ };
    var behavior = new ExceptionHandlingBehavior<CreateOrderCommand, Unit>(_logger);
    
    await Assert.ThrowsExceptionAsync<NullReferenceException>(
        () => behavior.Handle(command, 
            async () => throw new NullReferenceException(), 
            CancellationToken.None));
    
    _loggerMock.Verify(l => l.Log(LogLevel.Error, ...), Times.Once);
}
```

## Related Files

- **ExceptionHandlingBehavior**: `NYCTaxiData.Application\Behaviors\ExceptionHandlingBehavior.cs`
- **GlobalExceptionHandlerMiddleware**: `NYCTaxiData.API\MiddleWares\GlobalExceptionHandler.cs`
- **ValidationException**: `NYCTaxiData.Application\Common\Exceptions\ValidationException.cs`
- **UnauthorizedException**: `NYCTaxiData.Application\Common\Exceptions\UnauthorizedException.cs`

## Summary

The **ExceptionHandlingBehavior** provides:

✅ Centralized exception handling  
✅ Type-based exception categorization  
✅ Appropriate severity level logging  
✅ Exception context preservation  
✅ Integration with middleware  
✅ Custom exception types for domain errors  

It's essential for robust error handling and observability across the entire application.
