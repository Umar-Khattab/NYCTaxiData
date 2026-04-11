# Transaction Behavior Implementation Guide

## Overview
The `TransactionBehavior` is a MediatR pipeline behavior that automatically manages database transactions for command operations. It ensures data consistency and atomicity by wrapping request handlers in database transactions, committing on success and rolling back on failure.

## Components

### TransactionBehavior
Located in: `NYCTaxiData.Application\Behaviors\TransactionBehavior.cs`

This behavior handles database transaction management for all commands.

**Features:**
- Automatic transaction wrapping for commands
- Skips queries (naturally read-only)
- Automatic commit on success
- Automatic rollback on failure
- Cancellation handling
- Comprehensive logging
- Uses Unit of Work pattern

## How It Works

### Request Classification

```csharp
// Commands - Transactional (name ends with "Command")
public record CreateOrderCommand(...) : IRequest<OrderResult>;
                                    ↓
                        Applied transaction ✓

// Queries - Non-Transactional (name ends with "Query")
public record GetOrderQuery(...) : IRequest<OrderResult>;
                              ↓
                    Skipped - read-only ✗
```

### Execution Flow

```
REQUEST (Command)
    ↓
Is this a Command? → No → Skip transaction
                        ↓
                    Execute normally
                        
    ↓ Yes
Begin database transaction
    ↓
Execute request handler
    ↓
Handler performs operations (inserts, updates, deletes)
    ↓
Save changes to database
    ↓
On success:
  ├─ Commit transaction
  ├─ Log success
  └─ Return response

On failure:
  ├─ Rollback transaction
  ├─ Log error
  └─ Throw exception
    ↓
RESPONSE or ERROR
```

## Transaction Behavior

### Atomic Operations

```csharp
// Example: CreateOrderCommand
// All operations succeed or all fail together
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResult>
{
    public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        // Within transaction - all succeed or all fail
        var order = new Order { ... };
        _db.Orders.Add(order);
        
        var invoice = new Invoice { ... };
        _db.Invoices.Add(invoice);
        
        // If any fails, entire transaction rolls back
        await _db.SaveChangesAsync();
        
        return new OrderResult { OrderId = order.Id };
    }
}
```

## Integration Steps

### Step 1: Register in MediatR Configuration
```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Add TransactionBehavior as the last behavior (innermost layer)
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TimeoutBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>)); // ← NEW (last)
});
```

### Step 2: Register IUnitOfWork
```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### Step 3: That's It!
TransactionBehavior automatically manages transactions for all commands.

## Log Output Examples

### Successful Command with Transaction
```
[Debug] Beginning database transaction for request CreateOrderCommand
[Information] Database transaction committed successfully for request CreateOrderCommand
```

### Failed Command with Rollback
```
[Debug] Beginning database transaction for request CreateOrderCommand
[Error] Database transaction failed for request CreateOrderCommand. Error: Foreign key violation. Transaction will be rolled back.
System.InvalidOperationException: The referenced entity does not exist
```

### Query (Transaction Skipped)
```
[Debug] Request GetOrderQuery is not transactional, skipping transaction wrapper
```

### Cancelled Transaction
```
[Debug] Beginning database transaction for request UpdateOrderCommand
[Warning] Database transaction cancelled for request UpdateOrderCommand
System.OperationCanceledException: The operation was canceled.
```

## Best Practices

### 1. **Commands Should Be Transactional**
```csharp
// ✅ GOOD - Commands wrapped in transactions
public record CreateOrderCommand(...) : IRequest<OrderResult>;
public record UpdateOrderCommand(...) : IRequest<Unit>;
public record DeleteOrderCommand(...) : IRequest<Unit>;

// ✗ They're automatically transactional - no extra code needed
```

### 2. **Keep Transactions Short**
```csharp
// ✅ GOOD - Quick transaction
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResult>
{
    public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = new Order { ... };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();  // Commit happens here
        return new OrderResult { OrderId = order.Id };
    }
}

// ❌ BAD - Long transaction
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResult>
{
    public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = new Order { ... };
        _db.Orders.Add(order);
        await _externalApi.CallSlowService();  // Long operation - transaction still open
        await _db.SaveChangesAsync();
        return new OrderResult { OrderId = order.Id };
    }
}
```

### 3. **Avoid Nested Transactions**
```csharp
// ✅ GOOD - Single transaction level
public async Task CreateOrderWithInvoice(Order order, Invoice invoice)
{
    _db.Orders.Add(order);
    _db.Invoices.Add(invoice);
    await _db.SaveChangesAsync();  // Single transaction
}

// ❌ DIFFICULT - Nested transactions (EF Core handles this with savepoints)
public async Task CreateOrderWithInvoice(Order order, Invoice invoice)
{
    using (var transaction = await _db.Database.BeginTransactionAsync())
    {
        try
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();
            
            using (var nestedTransaction = await _db.Database.BeginTransactionAsync())
            {
                // Nested transaction - savepoint
                _db.Invoices.Add(invoice);
                await _db.SaveChangesAsync();
            }
        }
        catch
        {
            transaction.Rollback();
        }
    }
}
```

### 4. **Log Transaction Boundaries**
All transactions are automatically logged with:
- Transaction start message
- Success/failure status
- Error details on failure
- Execution duration (via LoggingBehavior)

### 5. **Respect Cancellation**
```csharp
// ✅ GOOD - Pass cancellation token through
public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
{
    await _db.Orders.AddAsync(order, ct);  // Respect cancellation
    await _db.SaveChangesAsync(ct);
    return result;
}

// ❌ BAD - Ignoring cancellation
public async Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken ct)
{
    await _db.Orders.AddAsync(order);  // No cancellation token
    return result;
}
```

## Pipeline Position

The recommended position for TransactionBehavior is:

```
1. LoggingBehavior         ← Log all requests
2. CachingBehavior         ← Check data cache
3. ValidationBehavior      ← Validate input
4. AuthorizationBehavior   ← Check permissions
5. IdempotencyBehavior     ← Handle duplicates
6. RetryBehavior           ← Retry failures
7. TimeoutBehavior         ← Enforce timeout
8. TransactionBehavior ← NEW ← Manage transactions (LAST/INNERMOST)
```

**Why this position?**
- Last/innermost so everything else happens OUTSIDE the transaction
- Transactions should only wrap the actual data operation
- Validation, auth, logging should NOT be inside transaction
- Ensures transactions are as short as possible

## Transaction vs. Unit of Work

| Aspect | Responsibility |
|--------|-----------------|
| **Transaction** | Atomicity - All or nothing |
| **Unit of Work** | Grouping - Multiple operations as one |
| **Combined** | Ensures atomic grouped operations |

```csharp
// UnitOfWork manages transaction lifecycle
await _unitOfWork.ExecuteInTransactionAsync(async () =>
{
    // All database operations here are atomic
    await _db.Orders.AddAsync(order);
    await _db.Invoices.AddAsync(invoice);
    // Both added or neither added
});

// SaveChangesAsync commits the transaction
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

## Isolation Levels

```csharp
// By default uses database default isolation level
// Typically: READ_COMMITTED (SQL Server, PostgreSQL)

// Current implementation uses framework default
// Can be customized if needed in IUnitOfWork.ExecuteInTransactionAsync

Isolation Levels (from lowest to highest):
  1. Read Uncommitted  - Dirty reads possible
  2. Read Committed    - No dirty reads (most common)
  3. Repeatable Read   - No dirty or non-repeatable reads
  4. Serializable      - Complete isolation (slowest)
```

## Testing Transactions

### Test 1: Successful Commit
```csharp
[TestMethod]
public async Task Handle_ValidCommand_CommitsTransaction()
{
    var command = new CreateOrderCommand(...);
    var handler = new CreateOrderHandler(_db);
    
    await handler.Handle(command, CancellationToken.None);
    
    // Verify order exists in database
    var order = await _db.Orders.FindAsync(command.OrderId);
    Assert.IsNotNull(order);
}
```

### Test 2: Rollback on Failure
```csharp
[TestMethod]
public async Task Handle_InvalidCommand_RollsBackTransaction()
{
    var command = new CreateOrderCommand
    {
        OrderId = "order-1",
        CustomerId = "invalid-id"  // Non-existent customer
    };
    
    var handler = new CreateOrderHandler(_db);
    
    // Should throw exception and rollback
    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        () => handler.Handle(command, CancellationToken.None));
    
    // Verify order was NOT created (rolled back)
    var order = await _db.Orders.FindAsync(command.OrderId);
    Assert.IsNull(order);
}
```

### Test 3: Multiple Operations Atomic
```csharp
[TestMethod]
public async Task Handle_MultipleOperations_AllSucceedOrAllFail()
{
    var order = new Order { ... };
    var invoice = new Invoice { ... };
    
    // If invoice creation fails, order should also be rolled back
    await Assert.ThrowsExceptionAsync<InvalidOperationException>(
        () => CreateOrderWithInvoiceAsync(order, invoice, throwOnInvoice: true));
    
    // Both should be rolled back
    Assert.IsNull(_db.Orders.Find(order.Id));
    Assert.IsNull(_db.Invoices.Find(invoice.Id));
}
```

## Related Files

- **TransactionBehavior**: `NYCTaxiData.Application\Behaviors\TransactionBehavior.cs`
- **IUnitOfWork**: `NYCTaxiData.Domain\Interfaces\IUnitOfWork.cs`
- **UnitOfWork**: `NYCTaxiData.Infrastructure\Services\UnitOfWork.cs`
- **LoggingBehavior**: For logging transaction events
- **DbContext**: For database configuration

## Common Issues & Solutions

### Issue: Nested Transaction Not Supported
**Symptoms**: Exception about nested transactions

**Solution**: 
- Use Savepoints (EF Core handles this)
- Simplify to single transaction
- Consider separating into separate commands

### Issue: Long Transaction Timeout
**Symptoms**: Commands timeout while in transaction

**Solution**:
1. Move long operations outside transaction
2. Increase command timeout
3. Optimize query performance
4. Split into smaller transactions

### Issue: Deadlocks in High Concurrency
**Symptoms**: Frequent deadlock exceptions

**Solution**:
1. Keep transactions short
2. Access tables in consistent order
3. Use appropriate isolation level
4. Add retry logic (RetryBehavior helps)

## Summary

The **TransactionBehavior** provides:

✅ Automatic transaction management  
✅ Atomic operations (all or nothing)  
✅ Automatic commit on success  
✅ Automatic rollback on failure  
✅ Cancellation support  
✅ Comprehensive logging  
✅ Unit of Work integration  

It's essential for maintaining data consistency and ensuring that complex operations either fully succeed or fully fail without leaving the database in an inconsistent state.
