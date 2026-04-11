# Transaction Behavior - Quick Reference

## What It Does

Automatically manages database transactions for commands:
- **Creates transaction** at request start
- **Commits transaction** on success
- **Rolls back transaction** on failure
- **Skips for queries** (read-only)

## Quick Example

```csharp
// TransactionBehavior handles this automatically
var command = new CreateOrderCommand(
    OrderId: "order-123",
    CustomerId: "cust-456",
    Items: new() { ... }
);

// Flow:
// 1. Begin transaction
// 2. Execute handler (creates order)
// 3. Success → Commit ✓
// Order persisted atomically
```

## When Applied

```
Commands → Transaction Applied ✓
  CreateOrderCommand
  UpdateOrderCommand
  DeleteOrderCommand

Queries → Skipped (read-only) ✗
  GetOrderQuery
  ListOrdersQuery
  SearchOrdersQuery
```

## Integration (2 Steps)

### Step 1: Register Behavior (Last)
```csharp
config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
```

### Step 2: Register IUnitOfWork
```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

That's it! Transactions work automatically.

## Log Output

### Successful Transaction
```
[Debug] Beginning database transaction for request CreateOrderCommand
[Information] Database transaction committed successfully
```

### Failed Transaction (Rollback)
```
[Debug] Beginning database transaction for request CreateOrderCommand
[Error] Database transaction failed. Error: Foreign key violation. Transaction will be rolled back.
```

### Query (Skipped)
```
[Debug] Request GetOrderQuery is not transactional, skipping transaction wrapper
```

## What Transactions Guarantee

✅ **Atomicity** - All or nothing
- Either all changes saved
- Or all rolled back
- No partial updates

✅ **Consistency** - Valid state
- Database rules enforced
- Foreign keys checked
- Constraints validated

✅ **Isolation** - No interference
- Other transactions don't see uncommitted changes
- Prevents dirty reads

✅ **Durability** - Permanent
- Committed changes survive failures

## Pipeline Position

```
REQUEST
  ↓
[1] LoggingBehavior ........... Log request
[2] CachingBehavior ........... Check cache
[3] ValidationBehavior ........ Validate input
[4] AuthorizationBehavior ..... Check permissions
[5] IdempotencyBehavior ....... Prevent duplicates
[6] RetryBehavior ............ Retry failures
[7] TimeoutBehavior .......... Enforce timeout
[8] TransactionBehavior ← HERE ... Manage transaction
  ↓
HANDLER (Database operations)
  ↓
RESPONSE
```

## Keep Transactions Short

```csharp
// ✅ GOOD - Quick transaction
public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
{
    var order = new Order { ... };
    _db.Orders.Add(order);
    await _db.SaveChangesAsync();  // Commit happens here
    return new OrderResult { OrderId = order.Id };
}

// ❌ BAD - Long transaction
public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
{
    var order = new Order { ... };
    _db.Orders.Add(order);
    await _externalApi.CallSlowService();  // Blocks transaction
    await _db.SaveChangesAsync();
    return new OrderResult { OrderId = order.Id };
}
```

## Atomicity Example

```csharp
// Both succeed or both fail
public async Task Handle(TransferMoneyCommand command, CancellationToken ct)
{
    var fromAccount = await _db.Accounts.FindAsync(command.FromId);
    var toAccount = await _db.Accounts.FindAsync(command.ToId);
    
    fromAccount.Balance -= command.Amount;
    toAccount.Balance += command.Amount;
    
    await _db.SaveChangesAsync();
}

Scenario 1: Success
  ├─ FromAccount.Balance decreased
  ├─ ToAccount.Balance increased
  └─ Transaction committed ✓

Scenario 2: Failure (insufficient funds)
  ├─ Both changes rolled back
  └─ Transaction rolled back ✗
```

## Isolation Levels

```
Default: READ_COMMITTED (most common)

Prevents:
  ✅ Dirty reads
  ✅ Lost updates
  ❌ Non-repeatable reads (possible)
  ❌ Phantom reads (possible)
```

## Best Practices

✅ **DO**:
- Keep transactions short
- Use database constraints
- Respect cancellation tokens
- Handle timeout errors

❌ **DON'T**:
- Call external APIs in transaction
- Perform long computations
- Manually manage transactions (automatic)
- Ignore rollback failures

## Related Documentation

- See `TRANSACTION_README.md` for complete guide
- See `INDEX.md` for all behaviors
- See `COMPLETE_IMPLEMENTATION.md` for full overview

## Build Status

✅ Build Successful - Ready to use!

## Key Takeaways

✅ Automatic transaction management  
✅ Atomic operations (all or nothing)  
✅ Automatic commit/rollback  
✅ Skips read-only queries  
✅ Cancellation support  

## Dependencies

- ✅ **MediatR** - Already installed
- ✅ **IUnitOfWork** - Already available
- ✅ **Logging** - Uses ILogger
- ✅ **Entity Framework Core** - Database layer
