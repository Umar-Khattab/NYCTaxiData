# Idempotency Behavior Implementation Guide

## Overview
The `IdempotencyBehavior` is a MediatR pipeline behavior that implements idempotency for command operations. It prevents duplicate operations by caching successful responses based on idempotency keys, protecting against network retries, client re-submissions, and other sources of duplicate requests.

## Components

### IdempotencyBehavior
Located in: `NYCTaxiData.Application\Behaviors\IdempotencyBehavior.cs`

This behavior detects duplicate requests and returns cached responses.

**Features:**
- Automatic idempotency key detection from requests
- Response caching for successful operations
- In-flight request tracking (concurrent duplicate detection)
- Automatic cache expiration (24 hours)
- JSON serialization/deserialization of responses
- Differential handling of commands vs. queries

### IIdempotencyService
Located in: `NYCTaxiData.Application\Common\Interfaces\IIdempotencyService.cs`

Service interface for managing idempotency keys and cached responses.

**Methods:**
- `GetCachedResponseAsync()` - Retrieve cached response
- `StoreCachedResponseAsync()` - Cache successful response
- `RemoveCachedResponseAsync()` - Remove cached response
- `IsProcessingAsync()` - Check if request is in-flight
- `MarkAsProcessingAsync()` - Mark request as being processed
- `ClearProcessingMarkerAsync()` - Clear in-flight marker

## How It Works

### Request Classification

```csharp
// Commands - Idempotent (name ends with "Command")
public record CreateOrderCommand(...) : IRequest<OrderResult>;
                                    ↓
                        Applied idempotency ✓

// Queries - Not Idempotent (name ends with "Query")
public record GetOrderQuery(...) : IRequest<OrderResult>;
                              ↓
                    Skipped - naturally idempotent ✗
```

### Execution Flow

```
REQUEST WITH IDEMPOTENCY KEY
    ↓
Is this a Command? → No → Skip idempotency
                        ↓
                    Execute normally
                        
    ↓ Yes
Get IdempotencyKey from request
    ↓
Is response cached? → Yes → Return cached response ✓
    ↓ No
Is request in-flight? → Yes → Throw error (prevent concurrent duplicates) ✗
    ↓ No
Mark as processing
    ↓
Execute request
    ↓
On success:
  ├─ Cache response
  └─ Clear in-flight marker
  
On failure:
  ├─ Don't cache (allow retry)
  └─ Clear in-flight marker
    ↓
RESPONSE
```

## Idempotency Key Extraction

The behavior looks for an idempotency key from the request in this order:

```csharp
1. IdempotencyKey property
   public record CreateOrderCommand(string IdempotencyKey, ...) : IRequest<...>;

2. Key property (fallback)
   public record CreateOrderCommand(string Key, ...) : IRequest<...>;

3. No key found → Skip idempotency
   public record CreateOrderCommand(...) : IRequest<...>;
```

## Integration Steps

### Step 1: Register IIdempotencyService Implementation

You'll need to implement `IIdempotencyService`. Example with Redis:

```csharp
// Using Redis
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
});

services.AddScoped<IIdempotencyService, RedisIdempotencyService>();
```

Or with in-memory cache:

```csharp
services.AddMemoryCache();
services.AddScoped<IIdempotencyService, MemoryIdempotencyService>();
```

### Step 2: Register in MediatR Configuration
```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Add IdempotencyBehavior (typically before retry/timeout)
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>)); // ← NEW
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TimeoutBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
});
```

### Step 3: Implement IIdempotencyService

Example Redis implementation:

```csharp
public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDistributedCache _cache;
    private const string ProcessingKeySuffix = ":processing";

    public RedisIdempotencyService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string?> GetCachedResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _cache.GetStringAsync($"idempotency:{idempotencyKey}", cancellationToken);
    }

    public async Task StoreCachedResponseAsync(string idempotencyKey, string response, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationTime ?? TimeSpan.FromHours(24)
        };

        await _cache.SetStringAsync($"idempotency:{idempotencyKey}", response, options, cancellationToken);
    }

    public async Task RemoveCachedResponseAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync($"idempotency:{idempotencyKey}", cancellationToken);
    }

    public async Task<bool> IsProcessingAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var value = await _cache.GetStringAsync($"idempotency:{idempotencyKey}{ProcessingKeySuffix}", cancellationToken);
        return !string.IsNullOrEmpty(value);
    }

    public async Task MarkAsProcessingAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        await _cache.SetStringAsync($"idempotency:{idempotencyKey}{ProcessingKeySuffix}", "processing", options, cancellationToken);
    }

    public async Task ClearProcessingMarkerAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync($"idempotency:{idempotencyKey}{ProcessingKeySuffix}", cancellationToken);
    }
}
```

## Usage Examples

### Example 1: Command with Idempotency

```csharp
// Define command with idempotency key
public record CreateOrderCommand(
    string IdempotencyKey,
    string CustomerId,
    List<OrderItem> Items) : IRequest<OrderResult>;

// Handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResult>
{
    public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Create order
        var order = new Order { CustomerId = request.CustomerId, Items = request.Items };
        await _db.Orders.AddAsync(order);
        await _db.SaveChangesAsync();
        
        return new OrderResult { OrderId = order.Id };
    }
}

// Client usage
var command = new CreateOrderCommand(
    IdempotencyKey: "order-123-retry-1",
    CustomerId: "cust-456",
    Items: new() { ... }
);

var result = await mediator.Send(command);
// First request: Creates order, caches result
// Second request (same key): Returns cached result immediately ✓
// Third request (same key): Returns cached result immediately ✓
```

### Example 2: Query (Skipped Idempotency)

```csharp
// Queries don't need idempotency (naturally safe)
public record GetOrderQuery(string OrderId) : IRequest<OrderResult>;

// Handler
public class GetOrderHandler : IRequestHandler<GetOrderQuery, OrderResult>
{
    public async Task<OrderResult> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.FindAsync(new { request.OrderId }, cancellationToken);
        return new OrderResult { OrderId = order.Id };
    }
}

// Idempotency is skipped - queries are naturally idempotent
```

### Example 3: Command Without Idempotency Key

```csharp
// If no idempotency key, behavior is bypassed
public record DeleteOrderCommand(string OrderId) : IRequest<Unit>;

// Handler
public class DeleteOrderHandler : IRequestHandler<DeleteOrderCommand, Unit>
{
    public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.FindAsync(new { request.OrderId }, cancellationToken);
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();
        
        return Unit.Value;
    }
}

// Usage without idempotency key - normal execution
var command = new DeleteOrderCommand("order-123");
await mediator.Send(command);
```

## Log Output Examples

### Successful Request (First Time)
```
[Debug] Processing request CreateOrderCommand with idempotency key order-123
[Information] Request CreateOrderCommand with idempotency key order-123 completed successfully
```

### Duplicate Request (Cached)
```
[Debug] Processing request CreateOrderCommand with idempotency key order-123
[Information] Request CreateOrderCommand with idempotency key order-123 returned cached response
```

### In-Flight Duplicate (Concurrent)
```
[Debug] Processing request CreateOrderCommand with idempotency key order-123
[Warning] Request CreateOrderCommand with idempotency key order-123 is already being processed
System.InvalidOperationException: Request with idempotency key order-123 is already being processed
```

### No Idempotency Key
```
[Debug] No idempotency key provided for CreateOrderCommand, executing without idempotency
```

### Query (Skipped)
```
[Debug] Request GetOrderQuery is not idempotent, skipping idempotency check
```

## HTTP Headers

### Request Header
```
POST /api/orders HTTP/1.1
Idempotency-Key: order-123-retry-1
Content-Type: application/json

{
  "idempotencyKey": "order-123-retry-1",
  "customerId": "cust-456",
  "items": [...]
}
```

### Response Header (Suggested)
```
HTTP/1.1 200 OK
Idempotency-Key: order-123-retry-1
X-Idempotency-Cached: true

{
  "orderId": "ord-789",
  "status": "created"
}
```

## Best Practices

### 1. **Use UUID for Idempotency Keys**
```csharp
// ✅ GOOD - Globally unique
var idempotencyKey = Guid.NewGuid().ToString();

// ❌ BAD - Not unique enough
var idempotencyKey = customerId;
```

### 2. **Require Idempotency for Write Operations**
```csharp
// ✅ GOOD - All commands have keys
public record CreateOrderCommand(string IdempotencyKey, ...) : IRequest<...>;
public record UpdateOrderCommand(string IdempotencyKey, ...) : IRequest<...>;
public record DeleteOrderCommand(string IdempotencyKey, ...) : IRequest<...>;

// ❌ BAD - Missing on some commands
public record CreateOrderCommand(string IdempotencyKey, ...) : IRequest<...>;
public record UpdateOrderCommand(...) : IRequest<...>;  // Missing!
```

### 3. **Document Idempotency in API**
```csharp
/// <summary>
/// Creates a new order.
/// This endpoint is idempotent - duplicate requests with the same Idempotency-Key
/// will return the same result without creating duplicate orders.
/// </summary>
/// <param name="command">The create order command with IdempotencyKey.</param>
/// <returns>The created order result.</returns>
[HttpPost("orders")]
public async Task<OrderResult> CreateOrder([FromBody] CreateOrderCommand command)
{
    return await _mediator.Send(command);
}
```

### 4. **Choose Appropriate Cache Duration**
```csharp
// ✅ GOOD - 24 hours for financial transactions
StoreCachedResponseAsync(key, response, TimeSpan.FromHours(24));

// ✅ GOOD - 1 hour for general operations
StoreCachedResponseAsync(key, response, TimeSpan.FromHours(1));

// ❌ BAD - Too short (60 seconds)
StoreCachedResponseAsync(key, response, TimeSpan.FromSeconds(60));

// ❌ BAD - Too long (30 days)
StoreCachedResponseAsync(key, response, TimeSpan.FromDays(30));
```

### 5. **Monitor Idempotency Cache**
```bash
# Monitor cache hit rate
grep "returned cached response" logs/log-*.txt | wc -l

# Check for concurrent duplicates
grep "already being processed" logs/log-*.txt | wc -l

# Monitor cache size
redis-cli INFO memory
```

## Pipeline Position

The recommended position for IdempotencyBehavior is:

```
1. LoggingBehavior         ← Log all requests
2. CachingBehavior         ← Check data cache
3. ValidationBehavior      ← Validate input
4. AuthorizationBehavior   ← Check permissions
5. IdempotencyBehavior ← NEW ← Handle duplicate requests
6. RetryBehavior           ← Retry transient failures
7. TimeoutBehavior         ← Enforce timeout
8. TransactionBehavior     ← Database transaction
```

**Why this position?**
- After validation (ensure valid before checking cache)
- After authorization (check permission before skipping execution)
- Before retry (retry may use same idempotency key)
- Before transaction (ensure duplicate detection before starting DB work)

## Idempotency vs. Caching

| Aspect | Idempotency | Caching |
|--------|-------------|---------|
| **Purpose** | Prevent duplicate side effects | Improve performance |
| **Scope** | Write operations (Commands) | Read operations (Queries) |
| **Duration** | Short-lived (minutes to hours) | Variable (seconds to days) |
| **Key** | Client-provided (Idempotency-Key) | System-generated hash |
| **Safety** | Ensures exactly-once semantics | Best-effort performance |

## Testing Idempotency

### Test 1: First Request Creates Order
```csharp
[TestMethod]
public async Task Handle_FirstRequest_CreatesOrder()
{
    var command = new CreateOrderCommand(
        IdempotencyKey: "order-1",
        CustomerId: "cust-1",
        Items: new() { ... }
    );

    var result1 = await mediator.Send(command);

    Assert.IsNotNull(result1);
    Assert.AreEqual("cust-1", result1.CustomerId);
}
```

### Test 2: Duplicate Request Returns Same Result
```csharp
[TestMethod]
public async Task Handle_DuplicateRequest_ReturnsCachedResult()
{
    var command = new CreateOrderCommand(
        IdempotencyKey: "order-1",
        CustomerId: "cust-1",
        Items: new() { ... }
    );

    var result1 = await mediator.Send(command);
    var result2 = await mediator.Send(command);  // Same key

    Assert.AreEqual(result1.OrderId, result2.OrderId);
    // Verify only one order was created in database
}
```

### Test 3: Concurrent Requests Prevented
```csharp
[TestMethod]
[ExpectedException(typeof(InvalidOperationException))]
public async Task Handle_ConcurrentRequest_ThrowsError()
{
    var command = new CreateOrderCommand(
        IdempotencyKey: "order-1",
        CustomerId: "cust-1",
        Items: new() { ... }
    );

    // Send simultaneously - second should fail
    var task1 = mediator.Send(command);
    var task2 = mediator.Send(command);

    await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => task2);
}
```

## Related Files

- **IdempotencyBehavior**: `NYCTaxiData.Application\Behaviors\IdempotencyBehavior.cs`
- **IIdempotencyService**: `NYCTaxiData.Application\Common\Interfaces\IIdempotencyService.cs`
- **LoggingBehavior**: For logging idempotency events
- **ValidationBehavior**: For validating requests before idempotency check

## Summary

The **IdempotencyBehavior** provides:

✅ Automatic duplicate request detection  
✅ Response caching for write operations  
✅ Concurrent duplicate prevention  
✅ Configurable cache duration  
✅ Seamless integration with MediatR  
✅ Production-ready implementation  

It's essential for building reliable APIs that handle network issues and client retries without creating duplicate side effects.
