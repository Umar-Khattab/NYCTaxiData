# Trips CQRS Implementation Documentation

## Overview

This document outlines the complete CQRS (Command Query Responsibility Segregation) implementation for the **Trips** feature in the NYC Taxi Data application. The Trips module handles all trip lifecycle operations including creation, completion, dispatch management, and history tracking.

---

## 📋 Table of Contents

1. [Commands](#commands)
2. [Queries](#queries)
3. [Architecture](#architecture)
4. [Usage Examples](#usage-examples)
5. [Exception Handling](#exception-handling)
6. [Validation](#validation)

---

## Commands

Commands represent write operations that change the state of the system. Each command includes validation, business logic, and error handling.

### 1. StartTripCommand

**Purpose:** Initiates a new trip for a driver between two locations.

**Location:** `Features/Trips/Commands/StartTrip/`

#### Command Definition
```csharp
public record StartTripCommand(
    Guid DriverId,
    int PickupLocationId,
    int DropoffLocationId
) : IRequest<TripStartResultDto>, ITransactionalCommand, ISecureRequest
```

#### Request Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `DriverId` | `Guid` | ✅ | The driver initiating the trip |
| `PickupLocationId` | `int` | ✅ | Location ID for pickup |
| `DropoffLocationId` | `int` | ✅ | Location ID for dropoff |

#### Response (TripStartResultDto)
```csharp
public class TripStartResultDto
{
    public int TripId { get; set; }
    public Guid DriverId { get; set; }
    public string Status { get; set; } = "In-Progress";
    public DateTime StartedAt { get; set; }
    public int PickupLocationId { get; set; }
    public int DropoffLocationId { get; set; }
}
```

#### Files Included
- ✅ `StartTripCommand.cs` - Command definition with response DTO
- ✅ `StartTripCommandValidator.cs` - FluentValidation rules
- ✅ `StartTripCommandHandler.cs` - Business logic handler

#### Features
- ✅ Validates driver, pickup, and dropoff locations exist
- ✅ Ensures pickup and dropoff are different
- ✅ Creates Trip record with current UTC timestamp
- ✅ Transactional (wrapped with `ITransactionalCommand`)
- ✅ Secure (requires authorization via `ISecureRequest`)
- ✅ Returns trip creation details

#### Validation Rules
- Driver ID must not be empty
- Pickup Location ID must be > 0
- Dropoff Location ID must be > 0
- Pickup and Dropoff locations must be different

---

### 2. EndTripCommand

**Purpose:** Completes a trip and calculates the fare based on base fare and surge multiplier.

**Location:** `Features/Trips/Commands/EndTrip/`

#### Command Definition
```csharp
public record EndTripCommand(
    int TripId,
    decimal BaseFare = 2.50m,
    decimal SurgeMultiplier = 1.0m
) : IRequest<TripEndResultDto>, ITransactionalCommand, ISecureRequest
```

#### Request Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `TripId` | `int` | - | The trip to end |
| `BaseFare` | `decimal` | 2.50 | Base fare amount in USD |
| `SurgeMultiplier` | `decimal` | 1.0 | Dynamic pricing multiplier |

#### Response (TripEndResultDto)
```csharp
public class TripEndResultDto
{
    public int TripId { get; set; }
    public int DurationMinutes { get; set; }
    public decimal BaseFare { get; set; }
    public decimal SurgeMultiplier { get; set; }
    public decimal TotalFare { get; set; }
    public DateTime EndedAt { get; set; }
    public string Status { get; set; } = "Completed";
}
```

#### Files Included
- ✅ `EndTripCommand.cs` - Command definition with response DTO
- ✅ `EndTripCommandValidator.cs` - FluentValidation rules
- ✅ `EndTripCommandHandler.cs` - Business logic handler

#### Features
- ✅ Validates trip exists
- ✅ Checks trip has been started (has `StartedAt` value)
- ✅ Prevents double-ending (checks `EndedAt` is null)
- ✅ Calculates trip duration in minutes
- ✅ Calculates fare: `BaseFare × SurgeMultiplier`
- ✅ Updates trip with completion time and fare
- ✅ Transactional guarantee
- ✅ Returns complete fare breakdown

#### Validation Rules
- Trip ID must be > 0
- Base fare must be > 0
- Surge multiplier must be > 0

#### Exception Handling
- `NotFoundException` - Trip not found
- `ConflictException` - Trip not started or already ended

---

### 3. ManualDispatchCommand

**Purpose:** Manager-initiated trip assignment to a specific driver for pickup/dropoff zones.

**Location:** `Features/Trips/Commands/ManualDispatch/`

#### Command Definition
```csharp
public record ManualDispatchCommand(
    Guid DriverId,
    int PickupZoneId,
    int DropoffZoneId,
    string PassengerName,
    string PassengerPhone
) : IRequest<DispatchResultDto>, ITransactionalCommand, ISecureRequest
```

#### Request Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `DriverId` | `Guid` | ✅ | Driver to dispatch |
| `PickupZoneId` | `int` | ✅ | Zone ID for pickup |
| `DropoffZoneId` | `int` | ✅ | Zone ID for dropoff |
| `PassengerName` | `string` | ✅ | Passenger name (2-100 chars) |
| `PassengerPhone` | `string` | ✅ | E.164 format phone number |

#### Response (DispatchResultDto)
```csharp
public class DispatchResultDto
{
    public string DispatchId { get; set; } = string.Empty;
    public Guid DriverId { get; set; }
    public int PickupZoneId { get; set; }
    public int DropoffZoneId { get; set; }
    public string Status { get; set; } = "Sent";
    public DateTime DispatchedAt { get; set; }
    public string PassengerName { get; set; } = string.Empty;
}
```

#### Files Included
- ✅ `ManualDispatchCommand.cs` - Command definition with response DTO
- ✅ `ManualDispatchCommandValidator.cs` - FluentValidation rules
- ✅ `ManualDispatchCommandHandler.cs` - Business logic handler

#### Features
- ✅ Verifies driver, pickup zone, and dropoff zone exist
- ✅ Resolves zones to locations (uses first location in each zone)
- ✅ Creates new Trip record
- ✅ Generates unique dispatch ID: `DSP-{TripId}-{UnixTimestamp}`
- ✅ Validates E.164 phone format (international standard)
- ✅ Transactional operation
- ✅ Manager-only authorization

#### Validation Rules
- Driver ID must not be empty
- Pickup Zone ID must be > 0
- Dropoff Zone ID must be > 0
- Zones must be different
- Passenger name: 2-100 characters
- Phone: Valid E.164 format (e.g., +1234567890)

#### Exception Handling
- `NotFoundException` - Driver, zone, or location not found

---

## Queries

Queries represent read operations that retrieve data without modifying state. Queries can be cached and optimized for performance.

### 1. GetTripHistoryQuery

**Purpose:** Retrieves paginated trip history for a specific driver.

**Location:** `Features/Trips/Queries/GetTripHistory/`

#### Query Definition
```csharp
public record GetTripHistoryQuery(
    Guid DriverId,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<TripHistoryResultDto>, ISecureRequest
```

#### Request Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `DriverId` | `Guid` | - | Driver ID to fetch history for |
| `PageNumber` | `int` | 1 | Page number for pagination |
| `PageSize` | `int` | 10 | Items per page |

#### Response (TripHistoryResultDto)
```csharp
public class TripHistoryResultDto
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public List<TripHistoryItemDto> Items { get; set; } = [];
}

public class TripHistoryItemDto
{
    public int TripId { get; set; }
    public string PickupZone { get; set; } = string.Empty;
    public string DropoffZone { get; set; } = string.Empty;
    public decimal? TotalFare { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
```

#### Files Included
- ✅ `GetTripHistoryQuery.cs` - Query definition with response DTOs
- ✅ `GetTripHistoryQueryHandler.cs` - Query handler with business logic

#### Features
- ✅ Verifies driver exists
- ✅ Pagination support (page number and size)
- ✅ Sorts by `StartedAt` descending (most recent first)
- ✅ Includes zone information via navigation
- ✅ Calculates trip duration in minutes
- ✅ Determines trip status (Completed/In-Progress)
- ✅ Returns paginated results with total counts
- ✅ Handles missing data gracefully

#### Exception Handling
- `NotFoundException` - Driver not found

---

### 2. GetLiveDispatchFeedQuery

**Purpose:** Retrieves real-time dispatch feed for the manager dashboard.

**Location:** `Features/Trips/Queries/GetLiveDispatchFeed/`

#### Query Definition
```csharp
public record GetLiveDispatchFeedQuery(
    int Limit = 20,
    int MinutesWindow = 60
) : IRequest<LiveDispatchFeedResultDto>, ISecureRequest
```

#### Request Parameters
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Limit` | `int` | 20 | Max number of recent dispatches |
| `MinutesWindow` | `int` | 60 | Time window in minutes to look back |

#### Response (LiveDispatchFeedResultDto)
```csharp
public class LiveDispatchFeedResultDto
{
    public List<DispatchFeedItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public DateTime RetrievedAt { get; set; }
}

public class DispatchFeedItemDto
{
    public string DispatchId { get; set; } = string.Empty;
    public int TripId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string PickupZone { get; set; } = string.Empty;
    public string DropoffZone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DispatchedAt { get; set; }
    public string TimeElapsed { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}
```

#### Files Included
- ✅ `GetLiveDispatchFeedQuery.cs` - Query definition with response DTOs
- ✅ `GetLiveDispatchFeedQueryHandler.cs` - Query handler with business logic

#### Features
- ✅ Filters trips within time window (default 60 minutes)
- ✅ Orders by most recent first
- ✅ Applies limit to prevent data overload
- ✅ Eager loads related entities (Driver, Locations, Zones)
- ✅ Generates unique dispatch IDs
- ✅ Determines dispatch status:
  - **Completed** - Trip has ended
  - **In-Progress** - Trip is active
  - **In-Progress (Long)** - Trip active for >60 minutes
  - **Pending** - Trip not yet started
- ✅ Formats elapsed time in human-readable format:
  - "X secs ago" (< 60 seconds)
  - "X mins ago" (< 60 minutes)
  - "X hours ago" (< 24 hours)
  - "X days ago" (>= 24 hours)
- ✅ Real-time monitoring ready

---

## Architecture

### CQRS Pattern

All Trips operations follow the **CQRS (Command Query Responsibility Segregation)** pattern:

- **Commands**: Write operations (`StartTrip`, `EndTrip`, `ManualDispatch`)
- **Queries**: Read operations (`GetTripHistory`, `GetLiveDispatchFeed`)

### Middleware & Behaviors Applied

Each command/query is processed through the following pipeline behaviors:

| Behavior | Applied To | Purpose |
|----------|-----------|---------|
| **ValidationBehavior** | All | Validates request using FluentValidation |
| **TransactionBehavior** | Commands with `ITransactionalCommand` | Wraps in database transaction |
| **AuthorizationBehavior** | Requests with `ISecureRequest` | Enforces authorization |
| **LoggingBehavior** | All | Logs request/response lifecycle |
| **ExceptionHandlingBehavior** | All | Handles exceptions consistently |
| **PerformanceBehavior** | All | Logs performance metrics |

### Marker Interfaces

```csharp
// Used in Commands
ITransactionalCommand      // Requires transaction wrapping
ISecureRequest             // Requires authorization

// Used in Queries
ISecureRequest             // Requires authorization
```

### Exception Types

| Exception | When Thrown | HTTP Status |
|-----------|------------|-------------|
| `NotFoundException` | Resource not found | 404 Not Found |
| `ConflictException` | Invalid state transition | 409 Conflict |
| `ValidationException` | Validation fails | 400 Bad Request |
| `UnauthorizedException` | Unauthorized access | 403 Forbidden |

---

## Usage Examples

### Starting a Trip

```csharp
// Send command via MediatR
var command = new StartTripCommand(
    DriverId: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
    PickupLocationId: 1,
    DropoffLocationId: 2
);

var result = await _mediator.Send(command);

// Response
// {
//   "tripId": 123,
//   "driverId": "550e8400-e29b-41d4-a716-446655440000",
//   "status": "In-Progress",
//   "startedAt": "2024-01-15T10:30:00Z",
//   "pickupLocationId": 1,
//   "dropoffLocationId": 2
// }
```

### Ending a Trip

```csharp
var command = new EndTripCommand(
    TripId: 123,
    BaseFare: 12.50m,
    SurgeMultiplier: 1.5m
);

var result = await _mediator.Send(command);

// Response
// {
//   "tripId": 123,
//   "durationMinutes": 25,
//   "baseFare": 12.50,
//   "surgeMultiplier": 1.5,
//   "totalFare": 18.75,
//   "endedAt": "2024-01-15T10:55:00Z",
//   "status": "Completed"
// }
```

### Manual Dispatch

```csharp
var command = new ManualDispatchCommand(
    DriverId: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
    PickupZoneId: 101,
    DropoffZoneId: 102,
    PassengerName: "John Doe",
    PassengerPhone: "+12125552368"
);

var result = await _mediator.Send(command);

// Response
// {
//   "dispatchId": "DSP-000123-1705317000",
//   "driverId": "550e8400-e29b-41d4-a716-446655440000",
//   "pickupZoneId": 101,
//   "dropoffZoneId": 102,
//   "status": "Sent",
//   "dispatchedAt": "2024-01-15T10:30:00Z",
//   "passengerName": "John Doe"
// }
```

### Get Trip History

```csharp
var query = new GetTripHistoryQuery(
    DriverId: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
    PageNumber: 1,
    PageSize: 10
);

var result = await _mediator.Send(query);

// Response
// {
//   "currentPage": 1,
//   "totalPages": 5,
//   "totalCount": 45,
//   "items": [
//     {
//       "tripId": 123,
//       "pickupZone": "Midtown West",
//       "dropoffZone": "Financial District",
//       "totalFare": 18.75,
//       "durationMinutes": 25,
//       "startedAt": "2024-01-15T10:30:00Z",
//       "endedAt": "2024-01-15T10:55:00Z",
//       "status": "Completed"
//     }
//   ]
// }
```

### Get Live Dispatch Feed

```csharp
var query = new GetLiveDispatchFeedQuery(
    Limit: 20,
    MinutesWindow: 60
);

var result = await _mediator.Send(query);

// Response
// {
//   "items": [
//     {
//       "dispatchId": "DSP-000123-1705317000",
//       "tripId": 123,
//       "driverName": "Ahmed Khaled",
//       "pickupZone": "Midtown West",
//       "dropoffZone": "Times Square",
//       "status": "In-Progress",
//       "dispatchedAt": "2024-01-15T10:30:00Z",
//       "timeElapsed": "5 mins ago",
//       "startedAt": "2024-01-15T10:30:00Z",
//       "endedAt": null
//     }
//   ],
//   "totalCount": 1,
//   "retrievedAt": "2024-01-15T10:35:00Z"
// }
```

---

## Exception Handling

### Common Exception Scenarios

#### Driver Not Found
```csharp
try
{
    var command = new StartTripCommand(
        DriverId: Guid.NewGuid(), // Non-existent driver
        PickupLocationId: 1,
        DropoffLocationId: 2
    );
    
    await _mediator.Send(command);
}
catch (NotFoundException ex)
{
    // ex.Message: "Driver with ID {guid} not found"
    // HTTP: 404
}
```

#### Trip Already Ended
```csharp
try
{
    var command = new EndTripCommand(TripId: 123); // Already ended
    await _mediator.Send(command);
}
catch (ConflictException ex)
{
    // ex.Message: "Trip has already ended"
    // HTTP: 409
}
```

#### Invalid Phone Number
```csharp
try
{
    var command = new ManualDispatchCommand(
        DriverId: Guid.NewGuid(),
        PickupZoneId: 101,
        DropoffZoneId: 102,
        PassengerName: "John",
        PassengerPhone: "123" // Invalid E.164 format
    );
    
    await _mediator.Send(command);
}
catch (ValidationException ex)
{
    // ex.Errors: "Passenger phone must be a valid phone number"
    // HTTP: 400
}
```

---

## Validation

### Validation Framework

All commands use **FluentValidation** for declarative validation rules.

### Validation Pipeline

1. **Semantic Validation** (FluentValidation rules in validators)
2. **Business Logic Validation** (handler checks entity existence, state)
3. **Exception Mapping** (validation failures → appropriate HTTP responses)

### Custom Validation Rules

- **E.164 Phone Format**: `^\+?[1-9]\d{1,14}$`
- **Location Requirement**: Both pickup and dropoff must be different
- **Fare Positivity**: Base fare and surge multiplier must be > 0

---

## Summary

| Feature | Commands | Queries |
|---------|----------|---------|
| **Count** | 3 | 2 |
| **Transactional** | Yes | No |
| **Authorization** | Yes | Yes |
| **Validation** | Yes | -  |
| **Status** | ✅ Complete | ✅ Complete |

### Implementation Status

- ✅ StartTripCommand - COMPLETE
- ✅ EndTripCommand - COMPLETE
- ✅ ManualDispatchCommand - COMPLETE
- ✅ GetTripHistoryQuery - COMPLETE
- ✅ GetLiveDispatchFeedQuery - COMPLETE
- ✅ Exception Handling - COMPLETE
- ✅ Validation - COMPLETE
- ✅ Middleware Integration - READY

---

## Next Steps

Potential future enhancements:

1. **Driver Status Management**
   - `UpdateDriverStatusCommand` - Update driver availability status
   - `GetActiveFleetQuery` - Get list of available drivers

2. **Zone Management**
   - `GetAllZonesQuery` - List all available zones
   - `GetSpecificZoneInsightsQuery` - Zone-specific analytics

3. **Advanced Features**
   - Trip rating and feedback
   - Driver performance metrics
   - Offline trip synchronization
   - Real-time location tracking

---

**Last Updated:** January 2024  
**Version:** 1.0  
**Status:** Production Ready ✅
