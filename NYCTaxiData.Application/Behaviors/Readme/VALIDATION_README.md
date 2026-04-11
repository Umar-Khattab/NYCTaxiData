# Validation Behavior Implementation Guide

## Overview
The `ValidationBehavior` is a MediatR pipeline behavior that automatically validates all incoming commands and queries using FluentValidation. It ensures data integrity before handlers are executed.

## Components

### 1. **ValidationException**
Located in: `NYCTaxiData.Application\Common\Exceptions\ValidationException.cs`

This exception is thrown when request validation fails.

**Properties:**
- `Errors`: Dictionary containing validation errors grouped by property name
  - Key: Property name (e.g., "PhoneNumber", "Password")
  - Value: Array of error messages for that property

**Usage:**
```csharp
try
{
    await mediator.Send(command);
}
catch (ValidationException ex)
{
    // ex.Errors contains grouped errors by property name
    foreach (var kvp in ex.Errors)
    {
        Console.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
    }
}
```

### 2. **ValidationBehavior**
Located in: `NYCTaxiData.Application\Behaviors\ValidationBehavior.cs`

The MediatR pipeline behavior that validates all requests.

**Features:**
- Auto-discovers and runs all validators for the request type
- Runs validators in parallel for performance
- Collects all failures before throwing exception
- Skips validation if no validators are registered

## How It Works

1. **Request arrives** → MediatR pipeline is triggered
2. **ValidationBehavior.Handle()** is called
3. **Check if validators exist** for the request type
   - If no validators → Skip to next behavior
   - If validators exist → Continue
4. **Create validation context** with the request
5. **Run all validators in parallel** using `Task.WhenAll`
6. **Collect all validation failures**
7. **If failures exist** → Throw `ValidationException` with all errors
8. **If no failures** → Execute the handler

## Creating Validators

Validators use FluentValidation and inherit from `AbstractValidator<T>`.

### Basic Example

```csharp
using FluentValidation;

namespace NYCTaxiData.Application.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        }
    }
}
```

### Common Validation Rules

```csharp
public class UserRegisterCommandValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterCommandValidator()
    {
        // String validations
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        // Phone validation
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number");

        // Numeric validations
        RuleFor(x => x.Age)
            .GreaterThan(0).WithMessage("Age must be positive")
            .LessThan(150).WithMessage("Age must be realistic");

        RuleFor(x => x.Salary)
            .GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative");

        // Collection validations
        RuleFor(x => x.Tags)
            .NotNull().WithMessage("Tags cannot be null")
            .NotEmpty().WithMessage("At least one tag is required");

        // Custom validation
        RuleFor(x => x.Password)
            .Must(BeAValidPassword).WithMessage("Password must contain uppercase, lowercase, and number");

        // Conditional validation
        RuleFor(x => x.AlternateEmail)
            .EmailAddress().WithMessage("Invalid alternate email")
            .When(x => !string.IsNullOrEmpty(x.AlternateEmail));
    }

    private bool BeAValidPassword(string password)
    {
        return password.Any(char.IsUpper) &&
               password.Any(char.IsLower) &&
               password.Any(char.IsDigit);
    }
}
```

### Advanced Validations

```csharp
public class UpdateDriverStatusCommandValidator : AbstractValidator<UpdateDriverStatusCommand>
{
    private readonly ICurrentUserService _currentUserService;

    public UpdateDriverStatusCommandValidator(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;

        RuleFor(x => x.DriverId)
            .NotEmpty().WithMessage("Driver ID is required");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(x => IsValidStatus(x)).WithMessage("Invalid driver status");

        // Async validation (e.g., check if driver exists)
        RuleFor(x => x.DriverId)
            .MustAsync(DriverExists).WithMessage("Driver does not exist");
    }

    private bool IsValidStatus(string status) =>
        new[] { "Active", "Offline", "OnBreak" }.Contains(status);

    private async Task<bool> DriverExists(Guid driverId, CancellationToken cancellationToken)
    {
        // Check database if driver exists
        return true; // Simplified
    }
}
```

### Custom Validators for Reusability

```csharp
public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> ValidatePhoneNumber<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number format");
    }

    public static IRuleBuilderOptions<T, string> ValidatePassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain digit");
    }
}

// Usage
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).ValidatePhoneNumber();
        RuleFor(x => x.Password).ValidatePassword();
    }
}
```

## Integration Steps

### Step 1: Register Validators in MediatR
Update the DependencyInjection.cs:

```csharp
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;

namespace NYCTaxiData.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // 1. Register all validators from the assembly
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // 2. Register MediatR with behaviors
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                
                // Register behaviors (order matters!)
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            });

            return services;
        }
    }
}
```

### Step 2: Register in Program.cs (if not using DependencyInjection static method)

```csharp
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
```

## Error Response Example

When validation fails, the API returns an error response like:

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation failures have occurred.",
    "status": 400,
    "traceId": "0HMVL7GKUS0B9:00000001",
    "errors": {
        "PhoneNumber": [
            "Phone number is required",
            "Invalid phone number format"
        ],
        "Password": [
            "Password is required",
            "Password must be at least 6 characters"
        ]
    }
}
```

## Global Exception Handler Integration

To properly handle `ValidationException` in your Global Exception Handler:

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                message = "One or more validation failures have occurred.",
                errors = validationException.Errors,
                statusCode = StatusCodes.Status400BadRequest
            }, cancellationToken);

            return true;
        }

        return false;
    }
}
```

## Best Practices

### 1. **Organize Validators with Commands/Queries**
```
Features/
  Auth/
    Commands/
      Login/
        LoginCommand.cs
        LoginCommandHandler.cs
        LoginCommandValidator.cs  ← Same folder as command
      Register/
        RegisterCommand.cs
        RegisterCommandHandler.cs
        RegisterCommandValidator.cs  ← Same folder as command
```

### 2. **Use Descriptive Error Messages**
```csharp
// ✅ Good
.NotEmpty().WithMessage("Email address is required to create an account")

// ❌ Bad
.NotEmpty().WithMessage("Email required")
```

### 3. **Validate at the Right Level**
```csharp
// ✅ Validate business rules with IDs only if necessary
RuleFor(x => x.ManagerId)
    .MustAsync(ManagerExists).WithMessage("Manager not found");

// ❌ Don't over-validate simple constraints
RuleFor(x => x.ManagerId)
    .NotEmpty()  // Redundant - MustAsync handles this
    .MustAsync(ManagerExists);
```

### 4. **Use Cascade Mode for Better UX**
```csharp
public class UserRegisterValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterValidator()
    {
        // Stop on first failure per property (avoid cascading errors)
        RuleSet("default", () =>
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email");
        });
    }
}
```

### 5. **Separate Complex Validations**
```csharp
public class CreateTripCommandValidator : AbstractValidator<CreateTripCommand>
{
    private readonly ITripService _tripService;

    public CreateTripCommandValidator(ITripService tripService)
    {
        _tripService = tripService;

        RuleFor(x => x.PickupLocation)
            .NotEmpty().WithMessage("Pickup location is required");

        RuleFor(x => x)
            .MustAsync(ValidateLocationsAreValid).WithMessage("Invalid trip route");

        RuleFor(x => x)
            .MustAsync(ValidateDriverAvailability).WithMessage("No available drivers");
    }

    private async Task<bool> ValidateLocationsAreValid(CreateTripCommand cmd, CancellationToken ct)
    {
        return await _tripService.ValidateLocations(cmd.PickupLocation, cmd.DropoffLocation, ct);
    }

    private async Task<bool> ValidateDriverAvailability(CreateTripCommand cmd, CancellationToken ct)
    {
        return await _tripService.HasAvailableDrivers(cmd.PickupLocation, ct);
    }
}
```

## Testing

```csharp
[TestClass]
public class LoginCommandValidatorTests
{
    private LoginCommandValidator _validator;

    [TestInitialize]
    public void Setup()
    {
        _validator = new LoginCommandValidator();
    }

    [TestMethod]
    public async Task Validate_PhoneNumberEmpty_ReturnsError()
    {
        // Arrange
        var command = new LoginCommand("", "password123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Any(e => e.PropertyName == "PhoneNumber"));
    }

    [TestMethod]
    public async Task Validate_InvalidPhoneNumber_ReturnsError()
    {
        // Arrange
        var command = new LoginCommand("invalid", "password123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public async Task Validate_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var command = new LoginCommand("+1234567890", "password123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }
}
```

## Pipeline Order (Recommended)

The suggested order for MediatR behaviors is:

1. **LoggingBehavior** - Log all requests (outermost)
2. **MetricsAndPerformanceBehavior** - Track performance
3. **CachingBehavior** - Return cached data if available
4. **ValidationBehavior** - Validate input ← **Current**
5. **AuthorizationBehavior** - Check permissions
6. **IdempotencyBehavior** - Prevent duplicate execution
7. **TransactionBehavior** - Begin/commit transactions (innermost)

## Performance Considerations

- Validators run in **parallel** using `Task.WhenAll` for better performance
- No validators registered = minimal overhead (single LINQ check)
- Async validations (database checks) are supported
- Consider using `RuleSet` for different validation scenarios

## Troubleshooting

### Issue: Validator not being executed
**Solution**: Ensure validator class name follows the pattern: `{CommandName}Validator` or `{QueryName}Validator`

### Issue: "No service for type IValidator<T>" error
**Solution**: Register validators using `AddValidatorsFromAssembly()` in DependencyInjection

### Issue: Validation errors not showing in response
**Solution**: Ensure Global Exception Handler catches `ValidationException` and formats it properly

### Issue: Performance degradation with many validators
**Solution**: Use async validators wisely, consider batching async operations

## Related Files

- **Validators**: `NYCTaxiData.Application\Features\**\*Validator.cs`
- **Global Exception Handler**: `NYCTaxiData.API\MiddleWares\GlobalExceptionHandler.cs`
- **Authorization Behavior**: See `AUTHORIZATION_README.md` for comparison
