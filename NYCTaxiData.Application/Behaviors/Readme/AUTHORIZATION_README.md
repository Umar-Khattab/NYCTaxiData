# Authorization Behavior Implementation Guide

## Overview
The `AuthorizationBehavior` is a MediatR pipeline behavior that enforces authorization checks on commands and queries. It ensures that only authenticated users with the appropriate roles can execute protected operations.

## Components

### 1. **AuthorizeAttribute**
Located in: `NYCTaxiData.Application\Common\Attributes\AuthorizeAttribute.cs`

This attribute marks commands/queries as requiring authorization.

**Parameters:**
- `roles` (optional): An array of `UserRole` values allowed to execute the request. If omitted, all authenticated users are allowed.

**Usage:**
```csharp
// Allow only authenticated users
[Authorize]
public record GetProfileQuery : IRequest<UserResultDto>
{
}

// Allow only specific roles (Drivers and Managers)
[Authorize(UserRole.Driver, UserRole.Manager)]
public record UpdateDriverStatusCommand(Guid DriverId, string Status) : IRequest<Unit>
{
}

// Allow only Managers
[Authorize(UserRole.Manager)]
public record UpdateSystemThresholdsCommand(int MaxThreshold, int MinThreshold) : IRequest<Unit>
{
}
```

### 2. **ICurrentUserService**
Located in: `NYCTaxiData.Application\Common\Interfaces\ICurrentUserService.cs`

This interface provides access to the current authenticated user's information.

**Properties:**
- `UserId`: The unique identifier of the current user
- `UserRole`: The role of the current user
- `PhoneNumber`: The phone number of the current user
- `IsAuthenticated`: A flag indicating whether the user is authenticated

**Implementation:**
You need to implement this interface in the Infrastructure layer to extract user information from the HTTP context (e.g., JWT claims, session data).

### 3. **UnauthorizedException**
Located in: `NYCTaxiData.Application\Common\Exceptions\UnauthorizedException.cs`

Custom exception thrown when authorization fails.

**Usage:**
```csharp
try
{
    await mediator.Send(command);
}
catch (UnauthorizedException ex)
{
    // Handle unauthorized access
}
```

### 4. **AuthorizationBehavior**
Located in: `NYCTaxiData.Application\Behaviors\AuthorizationBehavior.cs`

The actual MediatR pipeline behavior that performs authorization checks.

## How It Works

1. **Request arrives** → MediatR pipeline is triggered
2. **AuthorizationBehavior.Handle()** is called
3. **Check for [Authorize] attribute** on the request type
   - If not present → Skip authorization (allow all)
   - If present → Continue
4. **Verify user is authenticated**
   - If not → Throw `UnauthorizedException`
   - If yes → Continue
5. **Check required roles (if specified)**
   - If no roles specified → Allow all authenticated users
   - If roles specified → Verify user has one of the required roles
6. **If authorized** → Execute the handler
7. **If not authorized** → Throw `UnauthorizedException`

## Integration Steps

### Step 1: Register the Behavior in MediatR
Update the DependencyInjection.cs in the Application layer:

```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Add AuthorizationBehavior after Caching but before Validation
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
```

### Step 2: Implement ICurrentUserService
Create an implementation in the Infrastructure layer:

```csharp
namespace NYCTaxiData.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var claim = _httpContextAccessor?.HttpContext?.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(claim, out var result) ? result : null;
            }
        }

        public UserRole? UserRole
        {
            get
            {
                var claim = _httpContextAccessor?.HttpContext?.User?
                    .FindFirst("role")?.Value;
                return Enum.TryParse<UserRole>(claim, out var result) ? result : null;
            }
        }

        public string? PhoneNumber
        {
            get
            {
                return _httpContextAccessor?.HttpContext?.User?
                    .FindFirst(ClaimTypes.MobilePhone)?.Value;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
        }
    }
}
```

### Step 3: Register ICurrentUserService
In Program.cs or Infrastructure DependencyInjection:

```csharp
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();
```

## Example Usage

### Example 1: Public Command (No Authorization)
```csharp
public record LoginCommand(string PhoneNumber, string Password) 
    : IRequest<UserResultDto>
{
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, UserResultDto>
{
    // No authorization check - available to everyone
    public async Task<UserResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Example 2: Authenticated-Only Query
```csharp
[Authorize]
public record GetProfileQuery : IRequest<UserResultDto>
{
}

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserResultDto>
{
    private readonly ICurrentUserService _currentUserService;

    public async Task<UserResultDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        // Only authenticated users can reach here
        var userId = _currentUserService.UserId;
        // Implementation
    }
}
```

### Example 3: Role-Based Command
```csharp
[Authorize(UserRole.Manager)]
public record UpdateSystemThresholdsCommand(int MaxThreshold, int MinThreshold) 
    : IRequest<Unit>
{
}

public class UpdateSystemThresholdsCommandHandler 
    : IRequestHandler<UpdateSystemThresholdsCommand, Unit>
{
    public async Task<Unit> Handle(UpdateSystemThresholdsCommand request, CancellationToken cancellationToken)
    {
        // Only Managers can reach here
        // Implementation
    }
}
```

### Example 4: Multiple Roles
```csharp
[Authorize(UserRole.Driver, UserRole.Manager)]
public record UpdateDriverStatusCommand(Guid DriverId, string Status) 
    : IRequest<Unit>
{
}

public class UpdateDriverStatusCommandHandler 
    : IRequestHandler<UpdateDriverStatusCommand, Unit>
{
    public async Task<Unit> Handle(UpdateDriverStatusCommand request, CancellationToken cancellationToken)
    {
        // Both Drivers and Managers can reach here
        // Implementation
    }
}
```

## Pipeline Order (Recommended)

For optimal security and performance, register behaviors in this order:

1. **LoggingBehavior** - Log all requests (outermost)
2. **MetricsAndPerformanceBehavior** - Track performance
3. **CachingBehavior** - Return cached data if available
4. **AuthorizationBehavior** - Check permissions ← NEW
5. **ValidationBehavior** - Validate input data
6. **IdempotencyBehavior** - Prevent duplicate execution
7. **TransactionBehavior** - Begin/commit database transactions (innermost)

This order ensures:
- ✅ Cached data doesn't bypass authorization
- ✅ Invalid data doesn't trigger authorization checks
- ✅ Authorized and validated data goes into transactions

## Error Handling

The behavior throws `UnauthorizedException` in two scenarios:

### 1. User Not Authenticated
```
"User is not authenticated."
```

### 2. User Lacks Required Role
```
"User does not have the required role(s) to execute this action. Required roles: Manager, Admin"
```

Handle these exceptions in your Global Exception Handler:

```csharp
catch (UnauthorizedException ex)
{
    return Results.Forbid();
}
```

## Security Considerations

1. **JWT Claims**: Ensure user claims (ID, role, etc.) are properly validated and included in JWT tokens
2. **HTTPS**: Always use HTTPS in production to protect tokens in transit
3. **Token Expiry**: Implement token expiration and refresh token mechanisms
4. **Role Granularity**: Define roles based on actual business needs
5. **Audit Logging**: Consider logging all authorization failures for security audits

## Testing

```csharp
[TestClass]
public class AuthorizationBehaviorTests
{
    [TestMethod]
    public async Task Handle_UserNotAuthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var query = new GetProfileQuery();
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.Setup(x => x.IsAuthenticated).Returns(false);
        var behavior = new AuthorizationBehavior<GetProfileQuery, UserResultDto>(currentUserService.Object);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UnauthorizedException>(
            () => behavior.Handle(query, async () => new UserResultDto(), CancellationToken.None)
        );
    }

    [TestMethod]
    public async Task Handle_UserHasRequiredRole_ExecutesRequest()
    {
        // Arrange
        var query = new GetProfileQuery();
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());
        var behavior = new AuthorizationBehavior<GetProfileQuery, UserResultDto>(currentUserService.Object);

        // Act
        var result = await behavior.Handle(query, async () => new UserResultDto(), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
    }
}
```

## Troubleshooting

### Issue: Authorization check is skipped
**Solution**: Ensure the `[Authorize]` attribute is applied to the request class, not the handler.

### Issue: "User does not have required role" error
**Solution**: Check that:
1. The JWT token includes the correct role claim
2. The claim key matches what `ICurrentUserService` expects
3. The enum value matches the `UserRole` enum

### Issue: All requests require authentication
**Solution**: Only commands/queries marked with `[Authorize]` attribute require authentication. Commands without the attribute are public.
