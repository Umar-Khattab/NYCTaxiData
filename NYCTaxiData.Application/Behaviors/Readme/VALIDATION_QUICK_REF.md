# Validation Behavior - Quick Reference

## What Was Implemented

### 1. **ValidationException** ✅
Enhanced exception that groups validation errors by property:
```csharp
[Authorize]
public record UpdateDriverStatusCommand(Guid DriverId, string Status) 
    : IRequest<Unit> { }

// If validation fails:
try {
    await mediator.Send(command);
}
catch (ValidationException ex) {
    // ex.Errors["DriverId"] = ["Driver not found"]
    // ex.Errors["Status"] = ["Invalid status value"]
}
```

### 2. **ValidationBehavior** ✅
MediatR pipeline behavior that:
- Auto-discovers all FluentValidation validators
- Runs validators in parallel for performance
- Collects all failures before throwing
- Gracefully skips if no validators exist

## Flow Diagram

```
Request
  ↓
LoggingBehavior (Log request)
  ↓
CachingBehavior (Check cache)
  ↓
ValidationBehavior ← **NEW** (Run all validators in parallel)
  ├─ No validators? → Skip
  └─ Validators found?
     ├─ All valid? → Continue
     └─ Failures? → Throw ValidationException
  ↓
AuthorizationBehavior (Check user permissions)
  ↓
TransactionBehavior (Begin transaction)
  ↓
Handler (Execute business logic)
  ↓
TransactionBehavior (Commit transaction)
  ↓
Response
```

## Usage Examples

### Public Command (No Validation)
```csharp
public record LoginCommand(string PhoneNumber, string Password) 
    : IRequest<UserResultDto> { }
// Anyone can call this, validation happens via validator only
```

### With Validator
```csharp
// Command
public record LoginCommand(string PhoneNumber, string Password) 
    : IRequest<UserResultDto> { }

// Validator (automatically discovered and registered)
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number required")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password required")
            .MinimumLength(6).WithMessage("Min 6 characters");
    }
}

// Result when validation fails (400 Bad Request):
{
    "errors": {
        "PhoneNumber": ["Phone number required"],
        "Password": ["Min 6 characters"]
    }
}
```

## Key Features

✅ **Automatic Discovery**: Validators found via reflection  
✅ **Parallel Execution**: All validators run simultaneously  
✅ **Grouped Errors**: Errors organized by property name  
✅ **Graceful Handling**: Skips when no validators exist  
✅ **Async Support**: Supports async validation rules  
✅ **FluentAPI**: Rich, readable validation rules  

## Common Validation Rules

```csharp
public class UserRegisterValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterValidator()
    {
        // String rules
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);

        // Numeric rules
        RuleFor(x => x.Age)
            .GreaterThan(0)
            .LessThan(150);

        // Email
        RuleFor(x => x.Email)
            .EmailAddress();

        // Phone
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9]{10,15}$");

        // Conditionally
        RuleFor(x => x.Middle Initialname)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        // Custom logic
        RuleFor(x => x)
            .Must(x => x.Password != x.Username)
            .WithMessage("Password cannot be same as username");

        // Async (database checks)
        RuleFor(x => x.Email)
            .MustAsync(EmailNotExists)
            .WithMessage("Email already registered");
    }

    private async Task<bool> EmailNotExists(string email, CancellationToken ct)
    {
        // Check database
        return true;
    }
}
```

## Pipeline Order Summary

| Order | Behavior | Purpose |
|-------|----------|---------|
| 1 | LoggingBehavior | Log all requests |
| 2 | MetricsBehavior | Track performance |
| 3 | CachingBehavior | Return cached data |
| 4 | **ValidationBehavior** | **Validate input** ← NEW |
| 5 | AuthorizationBehavior | Check permissions |
| 6 | TransactionBehavior | Database transactions |

## Integration Checklist

- [x] ValidationException implemented
- [x] ValidationBehavior implemented  
- [x] Build successful
- [x] Documentation complete

## Next Steps

1. **Register in DependencyInjection.cs**:
   ```csharp
   services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
   config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
   ```

2. **Create validators** for your commands/queries using `AbstractValidator<T>`

3. **Test** with invalid data to see validation errors

4. **Update Global Exception Handler** to format ValidationException properly

## Common Patterns

### Reusable Custom Validators
```csharp
public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> ValidatePhoneNumber<T>(
        this IRuleBuilder<T, string> rule)
        => rule.Matches(@"^\+?[0-9]{10,15}$");

    public static IRuleBuilderOptions<T, string> ValidatePassword<T>(
        this IRuleBuilder<T, string> rule)
        => rule.MinimumLength(6).Matches(@"[A-Z]").Matches(@"[0-9]");
}

// Usage
RuleFor(x => x.PhoneNumber).ValidatePhoneNumber();
RuleFor(x => x.Password).ValidatePassword();
```

### Cross-Property Validation
```csharp
RuleFor(x => x)
    .Must(x => x.Password != x.Email)
    .WithMessage("Password cannot equal email");

RuleFor(x => x)
    .Must(x => x.EndDate > x.StartDate)
    .WithMessage("End date must be after start date");
```

### Conditional Validation
```csharp
RuleFor(x => x.AlternateEmail)
    .EmailAddress()
    .When(x => !string.IsNullOrEmpty(x.AlternateEmail));
```

## Related Documentation

- See **AUTHORIZATION_README.md** for authorization behavior
- See **Validators** in Features folder for examples
- FluentValidation docs: https://docs.fluentvalidation.net/

## Files Modified/Created

```
NYCTaxiData.Application/
├── Behaviors/
│   ├── ValidationBehavior.cs (IMPLEMENTED ✅)
│   └── VALIDATION_README.md (CREATED ✅)
├── Common/
│   └── Exceptions/
│       └── ValidationException.cs (IMPLEMENTED ✅)
└── Features/
    └── **/*Validator.cs (Already exist, auto-discovered)
```
