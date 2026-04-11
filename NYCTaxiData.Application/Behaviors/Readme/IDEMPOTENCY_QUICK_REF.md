# Idempotency Behavior - Quick Reference

## What It Does

Prevents duplicate operations by caching responses:
- **First request** - Creates order, caches response
- **Duplicate request** - Returns cached response (no side effects)
- **Concurrent duplicate** - Prevents concurrent duplicates

## Quick Example

```csharp
// Request 1 - Creates order
var command = new CreateOrderCommand(
    IdempotencyKey: "order-123",
    CustomerId: "cust-456",
    Items: new() { ... }
);
var result1 = await mediator.Send(command);
// Creates order in database, caches response

// Request 2 (same key) - Returns cached response
var result2 = await mediator.Send(command);
// Returns from cache, no database operation

// Result: Same response, one order created ✓
```

## Idempotency Key

```csharp
// Required for commands
public record CreateOrderCommand(
    string IdempotencyKey,  // ← Unique request identifier
    string CustomerId,
    List<OrderItem> Items
) : IRequest<OrderResult>;

// Optional - looks for:
// 1. IdempotencyKey property
// 2. Key property (fallback)
// 3. If none found - skip idempotency
```

## When Applied

```
Commands → Idempotency Applied ✓
  CreateOrderCommand
  UpdateOrderCommand
  DeleteOrderCommand

Queries → Skipped (naturally safe) ✗
  GetOrderQuery
  ListOrdersQuery
  SearchOrdersQuery
```

## Integration (3 Steps)

### Step 1: Implement IIdempotencyService
```csharp
services.AddMemoryCache();
services.AddScoped<IIdempotencyService, MemoryIdempotencyService>();

// Or use Redis
services.AddStackExchangeRedisCache(...);
services.AddScoped<IIdempotencyService, RedisIdempotencyService>();
```

### Step 2: Register Behavior
```csharp
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
```

### Step 3: Add IdempotencyKey to Commands
```csharp
public record CreateOrderCommand(
    string IdempotencyKey,  // ← Add this
    ...
) : IRequest<OrderResult>;
```

## Log Output

### First Request
```
[Debug] Processing request CreateOrderCommand with idempotency key order-123
[Information] Request CreateOrderCommand completed successfully
```

### Duplicate Request
```
[Debug] Processing request CreateOrderCommand with idempotency key order-123
[Information] Request CreateOrderCommand returned cached response
```

### No Idempotency Key
```
[Debug] No idempotency key provided for CreateOrderCommand, executing without idempotency
```

### Query (Skipped)
```
[Debug] Request GetOrderQuery is not idempotent, skipping idempotency check
```

## Cache Duration

```
Default: 24 hours

Configure:
  StoreCachedResponseAsync(key, response, TimeSpan.FromHours(1));
```

## HTTP Usage

### Request
```
POST /api/orders HTTP/1.1
Idempotency-Key: order-123-uuid

{
  "idempotencyKey": "order-123-uuid",
  "customerId": "cust-456",
  "items": [...]
}
```

### Response
```
HTTP/1.1 200 OK
Idempotency-Key: order-123-uuid

{
  "orderId": "ord-789",
  "status": "created"
}
```

## What Gets Cached

✅ Successful responses - Cached
❌ Failed responses - NOT cached (allow retry)
❌ Queries - Skipped (not cached by this behavior)

## Flow Diagram

```
REQUEST with IdempotencyKey
  ↓
Is it a Command? 
  ├─ No → Skip idempotency
  └─ Yes ↓
Is response cached?
  ├─ Yes → Return cached ✓
  └─ No ↓
Is already processing?
  ├─ Yes → Throw error ✗
  └─ No ↓
Mark as processing
  ↓
Execute request
  ↓
On success:
  ├─ Cache response
  └─ Clear processing
On failure:
  └─ Clear processing (no cache)
  ↓
RESPONSE
```

## Pipeline Position

```
REQUEST
  ↓
LoggingBehavior .......... Log start
  ↓
CachingBehavior ......... Check data cache
  ↓
ValidationBehavior ...... Validate input
  ↓
AuthorizationBehavior ... Check permissions
  ↓
IdempotencyBehavior ← HERE ... Handle duplicates
  ↓
RetryBehavior ........... Retry failures
  ↓
TimeoutBehavior ......... Enforce timeout
  ↓
HANDLER
```

## Best Practices

✅ **DO**:
- Use UUID for idempotency keys
- Include key in all write commands
- Document idempotency in API
- Monitor cache hit rate
- Use appropriate duration (1-24 hours)

❌ **DON'T**:
- Use sequential IDs as keys
- Skip key on some commands
- Use same key for different users
- Cache failed responses
- Use very short or very long cache durations

## Concurrent Requests

```
Request 1: POST /orders (key: order-123)
  ├─ Detected as processing
  │
Request 2: POST /orders (key: order-123) [concurrent]
  └─ Error: "already being processed" ✗

Solution: Wait for first request, then retry
```

## Duplicate Detection Mechanism

```
Processing Marker (TTL: 5 minutes):
  idempotency:order-123:processing = "processing"

Response Cache (TTL: 24 hours):
  idempotency:order-123 = "{serialized response}"

Workflow:
  1. Check if cached (return if exists)
  2. Check if processing (prevent concurrent)
  3. Mark as processing
  4. Execute
  5. Cache on success
  6. Clear processing marker
```

## Related Documentation

- See `IDEMPOTENCY_README.md` for complete guide
- See `INDEX.md` for all behaviors
- See `COMPLETE_IMPLEMENTATION.md` for full overview

## Build Status

✅ Build Successful - Ready to use!

## Key Takeaways

✅ Prevents duplicate operations  
✅ Caches responses for idempotent requests  
✅ Detects concurrent duplicates  
✅ Configurable cache duration  
✅ Seamless MediatR integration  

## Dependencies

- ✅ **MediatR** - Already installed
- ✅ **Logging** - Uses ILogger
- ⚠️ **IIdempotencyService** - Must implement
- ⚠️ **Cache Backend** - Redis or in-memory
