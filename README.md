# NYC Taxi Data — Complete Project Documentation

> **Repository:** https://github.com/Umar-Khattab/NYCTaxiData  
> **Framework:** .NET 10  
> **Architecture:** Clean Architecture (Domain → Application → Infrastructure → API)  
> **Database:** PostgreSQL  
> **Status:** Production Ready ✅

---

## Table of Contents

1. [What Is This Project?](#1-what-is-this-project)
2. [Architecture & Design Philosophy](#2-architecture--design-philosophy)
3. [Technology Stack](#3-technology-stack)
4. [Project Structure Explained](#4-project-structure-explained)
5. [Getting Started](#5-getting-started)
6. [Core Features Deep Dive](#6-core-features-deep-dive)
7. [Authentication & Authorization](#7-authentication--authorization)
8. [Database Design](#8-database-design)
9. [API Reference](#9-api-reference)
10. [Performance Monitoring](#10-performance-monitoring)
11. [Development Guide](#11-development-guide)
12. [Deployment](#12-deployment)
13. [Troubleshooting](#13-troubleshooting)

---

## 1. What Is This Project?

**NYC Taxi Data** is a full-stack, enterprise-grade backend system designed to manage and optimize New York City taxi operations. Think of it as the "brain" of a modern taxi fleet — it handles everything from dispatching trips and tracking drivers to predicting demand using machine learning.

### What Problems Does It Solve?

| Problem | Solution |
|---|---|
| **Manual dispatching is slow** | Real-time trip dispatching with live fleet tracking |
| **Drivers waste time waiting** | AI-powered demand forecasting tells drivers where to go |
| **No visibility into operations** | Analytics dashboards with KPIs and heatmaps |
| **Security concerns** | Multi-method authentication (password, OTP, OAuth, SAML, biometrics) |
| **System slowdowns go unnoticed** | Built-in real-time performance monitoring with automatic alerts |

### Who Is It For?

- **Fleet Managers** — Monitor drivers, view analytics, configure alert thresholds
- **Drivers** — Update status, manage trips, sync offline data
- **Data Scientists / AI Engineers** — Demand forecasting, operational simulations
- **System Administrators** — Monitor performance, manage security

---

## 2. Architecture & Design Philosophy

This project follows **Clean Architecture**, a design pattern that ensures the codebase remains maintainable, testable, and independent of external frameworks. The core idea is simple: **business logic should not depend on databases, web frameworks, or UI.**

### 2.1 The Four Layers

Imagine the system as a set of concentric circles, where the inner circles know nothing about the outer circles:

```
┌─────────────────────────────────────────┐
│    4. Presentation Layer (API)          │
│    Controllers, DTOs, Middleware        │
│    Depends on: Application, Domain      │
├─────────────────────────────────────────┤
│    3. Application Layer                 │
│    CQRS Commands/Queries, Behaviors,    │
│    Business Logic Orchestration         │
│    Depends on: Domain                   │
├─────────────────────────────────────────┤
│    2. Infrastructure Layer              │
│    Database access, External services   │
│    (Twilio, OAuth providers, etc.)      │
│    Depends on: Domain                   │
├─────────────────────────────────────────┤
│    1. Domain Layer (Core)               │
│    Entities, Enums, Interfaces, DTOs    │
│    Depends on: NOTHING                  │
└─────────────────────────────────────────┘
```

**Why this matters:** You could swap PostgreSQL for SQL Server, or swap Twilio for another SMS provider, and the business logic in the Domain and Application layers would not need to change.

### 2.2 CQRS Pattern — Separating Reads from Writes

**CQRS** stands for **Command Query Responsibility Segregation**. It's a pattern where:

- **Commands** = "Do something" (create a trip, update a driver, send an OTP)
- **Queries** = "Give me data" (list active drivers, get trip history, show KPIs)

Each command or query has its own dedicated **Handler** — a class that contains the exact logic for that operation.

**Example:** When a driver logs in:
1. The API receives a `LoginCommand`
2. MediatR routes it to `LoginCommandHandler`
3. The handler validates credentials, generates tokens, and returns the result

This separation makes the code easier to understand, test, and optimize.

### 2.3 The Pipeline — Automatic Cross-Cutting Concerns

When any command or query is executed, it passes through a **pipeline** of behaviors. These behaviors handle concerns that would otherwise clutter your business logic:

```
REQUEST enters the system
    ↓
[1] MetricsBehavior        → Start a stopwatch
[2] PerformanceBehavior    → Track if this request is slow
[3] LoggingBehavior        → Write to logs: "Processing LoginCommand"
[4] CachingBehavior        → Return cached result if available
[5] ValidationBehavior     → Check input using FluentValidation
[6] AuthorizationBehavior  → Verify the user has permission
[7] IdempotencyBehavior    → Prevent duplicate processing
[8] RetryBehavior          → Retry if database is temporarily down
[9] TimeoutBehavior        → Cancel if taking too long
[10] TransactionBehavior   → Wrap in a database transaction
    ↓
HANDLER executes the actual business logic
    ↓
RESPONSE returns to the caller
```

**The beauty of this:** Your `LoginCommandHandler` only contains login logic. It doesn't need to worry about logging, validation, transactions, or retries — the pipeline handles all of that automatically.

### 2.4 Repository Pattern — Database Abstraction

Instead of writing raw SQL or EF Core queries everywhere, the project uses the **Repository Pattern**:

- **`IGenericRepository<T>`** — Defines standard operations: `GetById`, `GetAll`, `Add`, `Update`, `Delete`
- **`GenericRepository<T>`** — Implements these using Entity Framework Core
- **`IUnitOfWork`** — Manages saving changes across multiple repositories in a single transaction

**Benefit:** The Application layer says "I need a user" without caring whether that user comes from PostgreSQL, a mock database for testing, or even a file.

---

## 3. Technology Stack

| Category | Technology | Purpose |
|---|---|---|
| **Framework** | .NET 10 | Core runtime and web framework |
| **ORM** | Entity Framework Core | Maps C# objects to database tables |
| **Database** | PostgreSQL (via Npgsql) | Primary data store |
| **CQRS Mediator** | MediatR | Routes commands/queries to handlers |
| **Validation** | FluentValidation | Declarative input validation |
| **Object Mapping** | AutoMapper | Converts between Entities and DTOs |
| **SMS/WhatsApp** | Twilio | Sends OTP codes to users |
| **Caching** | Microsoft.Extensions.Caching | In-memory response caching |
| **Authentication** | Custom + OAuth + SAML + WebAuthn | Multiple login methods |

---

## 4. Project Structure Explained

```
NYCTaxiData/
│
├── NYCTaxiData.Domain/              ← The "Heart" — pure business concepts
│   ├── Entities/                    ← Database table definitions
│   │   ├── Trip.cs                  ← A taxi trip (pickup, dropoff, fare)
│   │   ├── Driver.cs                ← Driver profile, status, vehicle
│   │   ├── Zone.cs                  ← NYC geographic zones
│   │   ├── User.cs                  ← Base user account
│   │   ├── Manager.cs               ← Manager profile & permissions
│   │   ├── DemandPrediction.cs      ← ML forecast results
│   │   ├── WeatherSnapshot.cs       ← Weather data for AI correlation
│   │   ├── SimulationRequest.cs     ← "What-if" scenario parameters
│   │   ├── SimulationResult.cs      ← Simulation outputs
│   │   └── ... (OAuth, SAML, WebAuthn entities)
│   ├── DTOs/                        ← Data Transfer Objects
│   │   └── Identity/
│   │       ├── LoginDto.cs          ← What the API expects for login
│   │       ├── RegistrationDto.cs   ← What the API expects for signup
│   │       ├── SendOtpDto.cs        ← Phone number for OTP
│   │       └── ...
│   ├── Enums/                       ← Fixed sets of values
│   │   ├── CurrentStatus.cs         ← Available, Busy, Offline, etc.
│   │   └── UserRole.cs              ← Manager, Driver
│   └── Interfaces/                  ← Contracts that other layers implement
│       ├── IGenericRepository.cs    ← "I promise I can do CRUD"
│       ├── IUnitOfWork.cs           ← "I promise I can save changes"
│       └── Identity/
│           ├── IAuthService.cs      ← "I promise I can authenticate users"
│           ├── ISmsService.cs       ← "I promise I can send SMS"
│           └── ICacheService.cs     ← "I promise I can cache data"
│
├── NYCTaxiData.Application/         ← The "Brain" — orchestrates everything
│   ├── Behaviors/                   ← Pipeline behaviors (see section 2.3)
│   │   ├── MetricsBehavior.cs
│   │   ├── PerformanceBehavior.cs   ⭐ Real-time monitoring
│   │   ├── LoggingBehavior.cs
│   │   ├── CachingBehavior.cs
│   │   ├── ValidationBehavior.cs
│   │   ├── AuthorizationBehavior.cs
│   │   ├── IdempotencyBehavior.cs
│   │   ├── RetryBehavior.cs
│   │   ├── TimeoutBehavior.cs
│   │   ├── TransactionBehavior.cs
│   │   └── ExceptionHandlingBehavior.cs
│   ├── Features/                    ← Organized by business domain
│   │   ├── Auth/                    ← Login, Register, OTP, Password Reset
│   │   │   ├── Commands/            ← Actions that change state
│   │   │   │   ├── Login/
│   │   │   │   │   ├── LoginCommand.cs
│   │   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   │   └── LoginCommandValidator.cs
│   │   │   │   ├── Register/
│   │   │   │   ├── SendOtp/
│   │   │   │   └── ...
│   │   │   └── Queries/             ← Actions that read state
│   │   │       └── GetProfile/
│   │   ├── Analytics/               ← Dashboards, KPIs, Charts
│   │   │   ├── Queries/
│   │   │   │   ├── GetTopLevelKpis/
│   │   │   │   ├── GetSystemThresholds/
│   │   │   │   └── GetDemandVelocityChart/
│   │   │   └── Commands/
│   │   │       └── UpdateSystemThresholds/
│   │   ├── AI/                      ← Machine Learning features
│   │   │   ├── Queries/
│   │   │   │   ├── GetDemandForecast/
│   │   │   │   ├── GetDispatchRecommendation/
│   │   │   │   ├── GetOptimalDriverSchedule/
│   │   │   │   └── GetExplainableAiInsight/
│   │   │   └── Commands/
│   │   │       ├── ProcessVoiceAssistantQuery/
│   │   │       ├── RunOperationalSimulation/
│   │   │       ├── RunStrategicSimulation/
│   │   │       └── TriggerModelRetraining/
│   │   ├── Trips/                   ← Trip lifecycle management
│   │   │   ├── Commands/
│   │   │   │   ├── StartTrip/
│   │   │   │   ├── EndTrip/
│   │   │   │   └── ManualDispatch/
│   │   │   └── Queries/
│   │   │       ├── GetLiveDispatchFeed/
│   │   │       └── GetTripHistory/
│   │   ├── Drivers/                 ← Driver fleet management
│   │   │   ├── Commands/
│   │   │   │   ├── UpdateDriverStatus/
│   │   │   │   └── SyncOfflineTrips/
│   │   │   └── Queries/
│   │   │       ├── GetActiveFleet/
│   │   │       └── GetShiftStatistics/
│   │   └── Zones/                   ← Geographic zone analytics
│   │       └── Queries/
│   │           ├── GetAllZones/
│   │           ├── GetLiveDemandHeatmap/
│   │           └── GetSpecificZoneInsights/
│   ├── Common/                      ← Shared utilities
│   │   ├── Interfaces/
│   │   │   ├── IApplicationDbContext.cs
│   │   │   ├── ICurrentUserService.cs
│   │   │   ├── IIdempotencyService.cs
│   │   │   ├── IAiPredictionService.cs
│   │   │   └── MarkerInterfaces/    ← "Tags" that enable behaviors
│   │   │       ├── IIdempotentCommand.cs
│   │   │       ├── ITransactionalCommand.cs
│   │   │       ├── ISecureRequest.cs
│   │   │       └── ICacheableQuery.cs
│   │   ├── Exceptions/              ← Custom error types
│   │   │   ├── ValidationException.cs
│   │   │   ├── UnauthorizedException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   └── ConflictException.cs
│   │   ├── Attributes/
│   │   │   └── AuthorizeAttribute.cs
│   │   └── Mappings/
│   │       └── MappingProfile.cs    ← AutoMapper configuration
│   └── DependencyInjection.cs       ← Registers Application services
│
├── NYCTaxiData.Infrastructure/      ← The "Muscle" — talks to the outside world
│   ├── Data/
│   │   ├── Contexts/
│   │   │   └── TaxiDbContext.cs     ← EF Core database context
│   │   ├── Repository/
│   │   │   └── GenericRepository.cs ← CRUD implementation
│   │   └── Initializers/
│   │       ├── IDbInitializers.cs
│   │       └── DbInitializers.cs    ← Seed data on first run
│   └── Services/
│       ├── AuthService.cs           ← Password hashing, token generation
│       ├── CacheService.cs          ← In-memory caching
│       ├── UnitOfWork.cs            ← Transaction management
│       └── Twilio/
│           ├── TwilioSettings.cs
│           └── WhatsAppSmsService.cs ← OTP delivery
│
└── NYCTaxiData.API/                 ← The "Face" — HTTP endpoints
    ├── Controllers/
    │   ├── AuthController.cs        ← /api/auth/*
    │   ├── DriversController.cs     ← /api/drivers/*
    │   ├── TripsController.cs       ← /api/trips/*
    │   ├── AnalyticsController.cs   ← /api/analytics/*
    │   ├── AiController.cs          ← /api/ai/*
    │   └── ZonesController.cs       ← /api/zones/*
    ├── MiddleWares/
    │   └── GlobalExceptionHandler.cs ← Catches all errors, returns nice responses
    ├── Contracts/
    │   └── APIResponse.cs           ← Standard wrapper for all responses
    ├── appsettings.json             ← Configuration (connection strings, API keys)
    └── Program.cs                   ← Application entry point
```

---

## 5. Getting Started

### 5.1 Prerequisites

Before you begin, ensure you have:

| Requirement | Version | Why |
|---|---|---|
| .NET SDK | 10.0+ | The runtime and compiler |
| PostgreSQL | 14+ | The database |
| Twilio Account | Any (free tier works) | For OTP/SMS features |
| Git | Any | To clone the repository |

### 5.2 Installation Steps

**Step 1: Clone the repository**
```bash
git clone https://github.com/Umar-Khattab/NYCTaxiData.git
cd NYCTaxiData
```

**Step 2: Configure the database connection**

Open `NYCTaxiData.API/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=nyctaxi;Username=your_username;Password=your_password"
  },
  "Twilio": {
    "AccountSid": "your_account_sid",
    "AuthToken": "your_auth_token",
    "FromNumber": "your_twilio_number"
  }
}
```

**Step 3: Create the database**

```bash
cd NYCTaxiData.API
dotnet ef database update
```

This command reads the migration files and creates all tables in PostgreSQL.

**Step 4: Build the solution**

```bash
dotnet build
```

**Step 5: Run the API**

```bash
cd NYCTaxiData.API
dotnet run
```

The API will start, typically at `https://localhost:5000` or `http://localhost:5001`.

### 5.3 Verify Everything Works

Test the login endpoint (even if it fails with "invalid credentials," a response means the API is running):

```bash
curl -X POST https://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'
```

You should receive a JSON response, confirming the API is operational.

---

## 6. Core Features Deep Dive

### 6.1 Authentication & User Management

The system supports **six different ways to log in**, making it suitable for everything from casual drivers to enterprise fleet managers:

| Method | How It Works | Best For |
|---|---|---|
| **Password** | Username + password, hashed with BCrypt | Primary login for most users |
| **OTP** | 6-digit code sent via SMS or WhatsApp | Quick verification, passwordless login |
| **OAuth 2.0** | "Sign in with Google/GitHub" | Social login convenience |
| **SAML 2.0** | Corporate single sign-on (SSO) | Enterprise fleet management |
| **WebAuthn** | Fingerprint, Face ID, or security key | Maximum security, passwordless |
| **MFA** | Combine any two methods above | High-security accounts |

**User Roles:**
- **`Manager`** — Full access: analytics, driver management, system settings, AI simulations
- **`Driver`** — Limited access: trip management, status updates, personal statistics, offline sync

### 6.2 Trip Management

The lifecycle of a taxi trip in the system:

1. **Start Trip** — A manager or the system assigns a pickup location, driver, and zone. The trip status becomes "Active."
2. **Live Tracking** — The trip appears in the live dispatch feed, visible to managers in real time.
3. **End Trip** — The driver or system records the drop-off location, final fare, and distance. Status becomes "Completed."
4. **Manual Dispatch** — Managers can override the system and manually assign trips to specific drivers.
5. **Offline Sync** — Drivers in areas with poor connectivity can record trips locally. When they reconnect, all data syncs to the server automatically.

### 6.3 Driver Management

- **Active Fleet View** — See all drivers currently on the road, their status, and location
- **Status Updates** — Drivers can set themselves as `Available`, `Busy`, `Offline`, or `On Break`
- **Shift Statistics** — Track hours worked, trips completed, earnings, and average ratings per shift
- **Offline Trip Upload** — Batch upload trips recorded without internet

### 6.4 Analytics & KPIs

The analytics module turns raw trip data into actionable insights:

- **Top-Level KPIs** — Total revenue today, total trips, active drivers right now, average customer wait time
- **System Thresholds** — Configurable alert limits (e.g., "Alert me if average wait time exceeds 5 minutes")
- **Demand Velocity Chart** — A time-series graph showing how taxi demand rises and falls throughout the day
- **Live Demand Heatmap** — A geographic map showing which NYC zones currently need the most taxis
- **Zone Insights** — Drill down into a specific zone to see historical trends, popular pickup times, and revenue potential

### 6.5 AI / Machine Learning

This is where the system goes beyond simple CRUD operations:

| Feature | What It Does | Business Value |
|---|---|---|
| **Demand Forecasting** | Predicts how many taxis will be needed in each zone for the next 2-24 hours | Drivers know where to position themselves |
| **Dispatch Recommendations** | Suggests the optimal driver for each incoming trip request | Reduces customer wait time |
| **Optimal Driver Scheduling** | Recommends shift start/end times based on predicted demand | Maximizes driver earnings |
| **Operational Simulations** | "What if it rains tomorrow?" — Simulates the impact of weather/events | Proactive planning |
| **Strategic Simulations** | "What if we add 50 more drivers to Zone 5?" — Long-term planning | Fleet expansion decisions |
| **Voice Assistant** | Drivers can ask "Where should I go?" and get a spoken recommendation | Hands-free operation |
| **Explainable AI** | Tells you *why* the AI recommended a specific action | Builds trust, enables debugging |
| **Model Retraining** | Trigger fresh training with the latest data | Keeps predictions accurate |

### 6.6 Performance Monitoring ⭐

Unlike most projects that require external tools like Prometheus or New Relic, this system has **built-in performance monitoring**:

**What It Tracks:**
- How long every single API request takes
- Rolling average of the last 100 requests per endpoint
- Whether performance is getting worse over time (degradation detection)

**Alert Levels:**

| Level | Query Threshold | Command Threshold | Meaning |
|---|---|---|---|
| 🟡 **SLOW** | > 500ms | > 1000ms | Worth investigating |
| 🟠 **WARNING** | > 1000ms | > 2000ms | Needs attention |
| 🔴 **CRITICAL** | > 5000ms | > 5000ms | System is struggling |

**Degradation Detection:** If the average response time for an endpoint increases by more than 20% compared to its historical average, the system flags it as "degrading."

**Accessing Data Programmatically:**

```csharp
// Get all operations that exceeded thresholds
var slowOps = PerformanceBehavior<object, object>.GetSlowOperations();

// Get operations that are getting slower over time
var degrading = PerformanceBehavior<object, object>.GetDegradingOperations();

// Get full history for a specific endpoint
var history = PerformanceBehavior<object, object>.GetPerformanceHistory("LoginCommand");

// Reset statistics (useful after a deployment)
PerformanceBehavior<object, object>.ResetPerformanceHistories();
```

---

## 7. Authentication & Authorization

### 7.1 How a Login Request Flows Through the System

```
1. User sends POST /api/auth/login
   { "username": "john_doe", "password": "SecurePass123!" }

2. AuthController creates a LoginCommand and sends it to MediatR

3. The Pipeline processes it automatically:
   ├─ ValidationBehavior checks: Is username provided? Is password at least 6 chars?
   ├─ AuthorizationBehavior skips (login is public)
   ├─ TransactionBehavior starts a database transaction
   └─ ...

4. LoginCommandHandler executes:
   ├─ Queries the database for user "john_doe"
   ├─ Verifies the password hash using BCrypt
   ├─ Generates a JWT access token (expires in 15 minutes)
   ├─ Generates a refresh token (expires in 7 days)
   ├─ Stores the refresh token in the database
   └─ Returns the user profile + both tokens

5. TransactionBehavior commits the transaction

6. PerformanceBehavior checks: Did this take longer than 500ms?

7. Response returns to the user:
   {
     "success": true,
     "data": {
       "accessToken": "eyJhbG...",
       "refreshToken": "dGhpcyBpcyBh...",
       "user": { "id": "...", "role": "Driver" }
     }
   }
```

### 7.2 Token Management

- **Access Token (JWT)** — Short-lived (15 minutes), used for every API call. Sent in the `Authorization: Bearer <token>` header.
- **Refresh Token** — Long-lived (7 days), stored in the database. Used to get a new access token when the old one expires.
- **OTP Token** — Single-use, expires quickly (typically 5 minutes). Sent via SMS/WhatsApp.
- **Session Tracking** — Every active login is recorded in the database, allowing managers to see who is logged in and revoke sessions if needed.

### 7.3 Authorization

There are two ways to protect endpoints:

1. **`[Authorize]` Attribute** — Placed on controllers or individual actions. Requires a valid JWT token.
2. **`ISecureRequest` Marker Interface** — When a Command or Query implements this interface, the `AuthorizationBehavior` automatically checks for a valid user before the handler runs.

**Example:**
```csharp
public class GetManagerDashboardQuery : IRequest<DashboardDto>, ISecureRequest
{
    // This query will fail with 401 if the user is not authenticated
}
```

---

## 8. Database Design

### 8.1 Entity Overview

The database is organized into logical groups:

#### User Management & Security
| Entity | Purpose |
|---|---|
| `User` | Base account (username, email, password hash) |
| `Manager` | Extended profile for fleet managers |
| `Driver` | Extended profile including vehicle info, license, current status |
| `Session` | Tracks active logins |
| `RefreshToken` | Stores token rotation for security |
| `OneTimeToken` | OTP codes with expiration timestamps |

#### Operations
| Entity | Purpose |
|---|---|
| `Trip` | Complete trip record: pickup/dropoff zones, timestamps, fare, distance, status |
| `Zone` | NYC taxi zones (official TLC zone definitions) |
| `Location` | Geographic coordinates (latitude/longitude) |

#### Analytics & AI
| Entity | Purpose |
|---|---|
| `DemandPrediction` | ML model outputs: predicted demand per zone per time window |
| `WeatherSnapshot` | Weather conditions at prediction time (rain, temperature, etc.) |
| `SimulationRequest` | Parameters for a "what-if" scenario |
| `SimulationResult` | Output metrics from running a simulation |
| `BucketsAnalytic` | Pre-aggregated data for fast dashboard loading |
| `BucketsVector` / `VectorIndex` | Vector embeddings for AI similarity search |

#### Enterprise Security (SSO)
| Entity | Purpose |
|---|---|
| `OauthClient` | Registered third-party OAuth applications |
| `OauthAuthorization` | OAuth grant records |
| `SamlProvider` | SAML identity provider configurations |
| `SsoDomain` | Domains allowed for corporate SSO |
| `WebauthnCredential` | Registered biometric/security key credentials |

#### File Storage
| Entity | Purpose |
|---|---|
| `Bucket` | S3-compatible storage container definitions |
| `S3MultipartUpload` | Tracks large file uploads in progress |
| `Object` | Metadata for stored files |

### 8.2 Resilience Configuration

The database connection is configured with automatic retry logic for transient failures:

```csharp
builder.Services.AddDbContext<TaxiDbContext>(options =>
{
    options.UseNpgsql(
        connectionString,
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,           // Try up to 3 times
            maxRetryDelay: TimeSpan.FromSeconds(5),  // Wait 5 seconds between tries
            errorCodesToAdd: null       // Use default transient error detection
        )
    );
});
```

**When this helps:** If PostgreSQL briefly restarts or the network hiccups, your API requests won't fail immediately — they'll wait and retry automatically.

---

## 9. API Reference

All endpoints return a consistent response format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... },
  "errors": null,
  "timestamp": "2026-04-25T17:38:00Z"
}
```

### 9.1 Authentication (`/api/auth`)

| Method | Endpoint | Description | Body |
|---|---|---|---|
| `POST` | `/api/auth/login` | Standard password login | `LoginDto` |
| `POST` | `/api/auth/register` | Create a new driver account | `RegistrationDto` |
| `POST` | `/api/auth/register-manager` | Create a new manager account | `ManagerRegisterDto` |
| `POST` | `/api/auth/send-otp` | Send OTP to phone via SMS/WhatsApp | `SendOtpDto` |
| `POST` | `/api/auth/verify-otp` | Verify OTP code and log in | `VerifyOtpDto` |
| `POST` | `/api/auth/reset-password` | Reset forgotten password | `ResetPasswordDto` |
| `POST` | `/api/auth/refresh-token` | Get new access token using refresh token | `RefreshTokenDto` |
| `GET` | `/api/auth/profile` | Get current user's profile | — |

### 9.2 Drivers (`/api/drivers`)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/drivers/active` | List all currently active drivers |
| `GET` | `/api/drivers/shift-stats` | Get statistics for the current or last shift |
| `PUT` | `/api/drivers/status` | Update driver availability status |
| `POST` | `/api/drivers/sync-offline` | Upload trips recorded while offline |

### 9.3 Trips (`/api/trips`)

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/trips/start` | Begin a new trip |
| `POST` | `/api/trips/end` | Complete an active trip |
| `POST` | `/api/trips/manual-dispatch` | Manually assign a trip to a driver |
| `GET` | `/api/trips/live-feed` | Real-time stream of active dispatches |
| `GET` | `/api/trips/history` | Searchable trip history |

### 9.4 Analytics (`/api/analytics`)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/analytics/kpis` | Top-level key performance indicators |
| `GET` | `/api/analytics/thresholds` | Current alert threshold settings |
| `GET` | `/api/analytics/demand-velocity` | Time-series data for demand charts |
| `PUT` | `/api/analytics/thresholds` | Update alert thresholds |

### 9.5 AI (`/api/ai`)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/ai/demand-forecast` | Get predicted demand for zones |
| `GET` | `/api/ai/dispatch-recommendation` | AI-suggested driver-trip assignments |
| `GET` | `/api/ai/driver-schedule` | Optimal shift recommendations |
| `GET` | `/api/ai/explain` | Explanation for an AI decision |
| `POST` | `/api/ai/voice-query` | Process a voice command |
| `POST` | `/api/ai/simulation/operational` | Run a short-term "what-if" simulation |
| `POST` | `/api/ai/simulation/strategic` | Run a long-term planning simulation |
| `POST` | `/api/ai/retrain` | Trigger model retraining with latest data |

### 9.6 Zones (`/api/zones`)

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/zones` | List all NYC taxi zones |
| `GET` | `/api/zones/heatmap` | Current demand heatmap data |
| `GET` | `/api/zones/{id}/insights` | Detailed analytics for a specific zone |

---

## 10. Performance Monitoring

### 10.1 How It Works (Under the Hood)

The `PerformanceBehavior` is a MediatR pipeline behavior that wraps every request. Here's what happens:

1. **Before the handler runs:** It records the current timestamp.
2. **After the handler completes:** It calculates the duration.
3. **It stores this measurement** in a thread-safe, fixed-size collection (last 100 measurements per endpoint).
4. **It compares** the current duration against thresholds and historical averages.
5. **If thresholds are exceeded,** it logs a warning with the endpoint name and duration.

### 10.2 Threshold Reference

```
READ OPERATIONS (Queries):
  🟡 SLOW     > 500ms   → Log warning
  🟠 WARNING  > 1000ms  → Log warning, flag for investigation
  🔴 CRITICAL > 5000ms  → Log error, immediate attention needed

WRITE OPERATIONS (Commands):
  🟡 SLOW     > 1000ms  → Log warning
  🟠 WARNING  > 2000ms  → Log warning, flag for investigation
  🔴 CRITICAL > 5000ms  → Log error, immediate attention needed

DEGRADATION:
  Triggered when current average exceeds historical average by 20% or more
```

### 10.3 Why This Matters

Without this system, you would only know about slowdowns when users complain. With it, you can:
- See exactly which endpoints are slow
- Detect performance degradation before it becomes critical
- Make data-driven optimization decisions (e.g., "LoginCommand is always slow — let's add a database index")

### 10.4 Performance Overhead

- **Pipeline overhead:** ~4ms total for all behaviors combined
- **Performance tracking alone:** ~1ms per request
- **Impact:** Less than 1% of typical request time
- **Memory usage:** Fixed — only the last 100 measurements per endpoint are kept

---

## 11. Development Guide

### 11.1 Adding a New Feature: Step-by-Step Example

Let's say you want to add a **"Cancel Trip"** feature. Here's exactly how to do it:

#### Step 1: Update the Domain Entity (if needed)

```csharp
// NYCTaxiData.Domain/Entities/Trip.cs
public class Trip
{
    // ... existing properties ...

    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public TripStatus Status { get; set; } // Add 'Cancelled' to the enum
}
```

#### Step 2: Create the Command

```csharp
// NYCTaxiData.Application/Features/Trips/CancelTrip/CancelTripCommand.cs
public class CancelTripCommand : IRequest<APIResponse<bool>>, ITransactionalCommand
{
    public Guid TripId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
```

**What this means:**
- It returns a boolean wrapped in our standard API response
- `ITransactionalCommand` tells the pipeline to wrap this in a database transaction

#### Step 3: Create the Handler

```csharp
// NYCTaxiData.Application/Features/Trips/CancelTrip/CancelTripCommandHandler.cs
public class CancelTripCommandHandler : IRequestHandler<CancelTripCommand, APIResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Trip> _tripRepository;

    public CancelTripCommandHandler(IUnitOfWork unitOfWork, IGenericRepository<Trip> tripRepository)
    {
        _unitOfWork = unitOfWork;
        _tripRepository = tripRepository;
    }

    public async Task<APIResponse<bool>> Handle(CancelTripCommand request, CancellationToken cancellationToken)
    {
        // Find the trip
        var trip = await _tripRepository.GetByIdAsync(request.TripId);

        if (trip == null)
            throw new NotFoundException($"Trip {request.TripId} not found");

        // Business logic: Can we cancel this trip?
        if (trip.Status == TripStatus.Completed)
            throw new ConflictException("Cannot cancel a completed trip");

        // Update the trip
        trip.Status = TripStatus.Cancelled;
        trip.CancellationReason = request.Reason;
        trip.CancelledAt = DateTime.UtcNow;

        _tripRepository.Update(trip);
        await _unitOfWork.SaveChangesAsync();

        return APIResponse<bool>.Success(true, "Trip cancelled successfully");
    }
}
```

#### Step 4: Create the Validator

```csharp
// NYCTaxiData.Application/Features/Trips/CancelTrip/CancelTripCommandValidator.cs
public class CancelTripCommandValidator : AbstractValidator<CancelTripCommand>
{
    public CancelTripCommandValidator()
    {
        RuleFor(x => x.TripId)
            .NotEmpty()
            .WithMessage("Trip ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Reason is required and must be under 500 characters");
    }
}
```

**What this does:** Before the handler ever runs, FluentValidation checks these rules. If they fail, the pipeline returns a 400 Bad Request automatically.

#### Step 5: Add the API Endpoint

```csharp
// NYCTaxiData.API/Controllers/TripsController.cs
[HttpPost("cancel")]
public async Task<ActionResult<APIResponse<bool>>> CancelTrip(CancelTripCommand command)
{
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

#### Step 6: Register Services (usually auto-discovered)

In `Program.cs`, MediatR automatically discovers handlers from the Application assembly:

```csharp
services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>());
```

**And you're done!** The new endpoint is live at `POST /api/trips/cancel`, fully validated, transactional, logged, and performance-monitored — all without writing any of that cross-cutting code yourself.

### 11.2 Code Conventions

| Element | Convention | Example |
|---|---|---|
| Classes | PascalCase | `CancelTripCommand` |
| Methods | PascalCase | `HandleAsync` |
| Properties | PascalCase | `TripId` |
| Private fields | camelCase with `_` prefix | `_tripRepository` |
| Files | Match class name exactly | `CancelTripCommand.cs` |
| Namespaces | Match folder structure | `NYCTaxiData.Application.Features.Trips.CancelTrip` |

### 11.3 Marker Interfaces — Your "Feature Switches"

These interfaces act as tags. When a command or query implements them, specific pipeline behaviors activate automatically:

| Interface | What Happens |
|---|---|
| `IIdempotentCommand` | The system checks if this exact request was already processed (prevents double-charging, double-booking, etc.) |
| `ITransactionalCommand` | The handler is wrapped in a database transaction. If it fails, all changes are rolled back. |
| `ISecureRequest` | The user must be authenticated. Returns 401 if no valid token is present. |
| `ICacheableQuery` | The response is cached for a configured duration. Subsequent identical requests return instantly. |

**Example:**
```csharp
public class CancelTripCommand : IRequest<APIResponse<bool>>, ITransactionalCommand, ISecureRequest
{
    // This will: require login, run in a transaction, and be fully monitored
}
```

---

## 12. Deployment

### 12.1 Docker Deployment (Recommended for Production)

Create a `Dockerfile` in `NYCTaxiData.API/`:

```dockerfile
# Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build image with SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["NYCTaxiData.API/NYCTaxiData.API.csproj", "NYCTaxiData.API/"]
COPY ["NYCTaxiData.Application/NYCTaxiData.Application.csproj", "NYCTaxiData.Application/"]
COPY ["NYCTaxiData.Domain/NYCTaxiData.Domain.csproj", "NYCTaxiData.Domain/"]
COPY ["NYCTaxiData.Infrastructure/NYCTaxiData.Infrastructure.csproj", "NYCTaxiData.Infrastructure/"]
RUN dotnet restore "NYCTaxiData.API/NYCTaxiData.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/NYCTaxiData.API"
RUN dotnet build "NYCTaxiData.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "NYCTaxiData.API.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NYCTaxiData.API.dll"]
```

Create `docker-compose.yml` in the root:

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: NYCTaxiData.API/Dockerfile
    ports:
      - "5000:80"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=nyctaxi;Username=postgres;Password=your_secure_password
      - Twilio__AccountSid=${TWILIO_SID}
      - Twilio__AuthToken=${TWILIO_TOKEN}
    depends_on:
      - postgres

  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: nyctaxi
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: your_secure_password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

**Deploy:**
```bash
docker-compose up -d
```

### 12.2 Production Checklist

Before going live, verify:

- [ ] **Database Security** — Strong PostgreSQL credentials, restricted network access
- [ ] **HTTPS** — Valid SSL certificate configured
- [ ] **Secrets Management** — API keys and connection strings stored in environment variables, never in code
- [ ] **Logging** — Configured to send logs to an external service (Seq, ELK, CloudWatch)
- [ ] **Health Checks** — Endpoint to verify API and database connectivity
- [ ] **Rate Limiting** — Prevent abuse of authentication endpoints
- [ ] **CORS** — Restrict to specific frontend origins only
- [ ] **Backups** — Automated PostgreSQL backups configured
- [ ] **Monitoring** — Review performance metrics regularly
- [ ] **Alerts** — Configure notifications for CRITICAL performance levels

---

## 13. Troubleshooting

### 13.1 Common Issues

| Symptom | Likely Cause | Solution |
|---|---|---|
| `Database connection failed` on startup | Wrong connection string or PostgreSQL not running | Check `appsettings.json`; ensure PostgreSQL service is active |
| Pipeline behaviors not executing | Behaviors not registered in DI | Verify behavior registration in `Program.cs` |
| Validators not catching bad input | FluentValidation not registered | Add `services.AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>()` |
| API responses are very slow | Missing database indexes | Add indexes on frequently queried columns (`Trip.DriverId`, `Driver.Status`, etc.) |
| OTP messages not arriving | Invalid Twilio credentials | Verify Twilio SID, Auth Token, and From Number in configuration |
| `401 Unauthorized` on protected endpoints | Missing or expired token | Check that `Authorization: Bearer <token>` header is present and valid |
| Cache not working | Query doesn't implement `ICacheableQuery` | Add the interface to your query class |

### 13.2 Performance Tuning Tips

1. **Add Database Indexes** — If `GetActiveFleet` is slow, add an index on `Driver.Status`
2. **Use Caching** — Mark read-heavy queries with `ICacheableQuery` to avoid repeated database hits
3. **Monitor Connection Pooling** — PostgreSQL connection pooling is enabled by default, but monitor for exhaustion under high load
4. **Optimize Behaviors** — If you don't need performance monitoring in a specific environment, you can conditionally remove `PerformanceBehavior` from the pipeline

### 13.3 Debugging Performance Issues

```csharp
// In a controller, service, or startup check:

// 1. See what's currently slow
var slowOps = PerformanceBehavior<object, object>.GetSlowOperations();
foreach (var op in slowOps)
{
    Console.WriteLine($"🟡 SLOW: {op.RequestName} took {op.DurationMs}ms");
}

// 2. See what's degrading over time
var degrading = PerformanceBehavior<object, object>.GetDegradingOperations();
foreach (var op in degrading)
{
    Console.WriteLine($"🔴 DEGRADING: {op.RequestName} is {op.DegradationPercentage}% slower than usual");
}

// 3. Check specific endpoint history
var history = PerformanceBehavior<object, object>.GetPerformanceHistory("GetTopLevelKpisQuery");
```

---

## Appendix A: File Naming Cheat Sheet

| Component | File Name Pattern | Example |
|---|---|---|
| Command | `{Name}Command.cs` | `CancelTripCommand.cs` |
| Command Handler | `{Name}CommandHandler.cs` | `CancelTripCommandHandler.cs` |
| Command Validator | `{Name}CommandValidator.cs` | `CancelTripCommandValidator.cs` |
| Query | `{Name}Query.cs` | `GetActiveFleetQuery.cs` |
| Query Handler | `{Name}QueryHandler.cs` | `GetActiveFleetQueryHandler.cs` |
| DTO | `{Name}Dto.cs` | `LoginDto.cs` |
| Entity | `{Name}.cs` | `Trip.cs` |
| Repository | `{Name}Repository.cs` | `TripRepository.cs` |
| Service | `{Name}Service.cs` | `AuthService.cs` |
| Controller | `{Name}Controller.cs` | `TripsController.cs` |
| Behavior | `{Name}Behavior.cs` | `PerformanceBehavior.cs` |
| Exception | `{Name}Exception.cs` | `NotFoundException.cs` |

---

## Appendix B: Complete Request Flow Example (Login)

Here's exactly what happens when a user logs in, from HTTP request to database and back:

```
STEP 1: HTTP Request
POST https://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "john",
  "password": "SecurePass123!"
}

    ↓

STEP 2: Controller Receives Request
AuthController.Login() creates a LoginCommand and calls _mediator.Send(command)

    ↓

STEP 3: MediatR Dispatches to Pipeline
The request enters the behavior pipeline in order:

[MetricsBehavior]        → Start stopwatch
[PerformanceBehavior]    → Initialize performance tracking for "LoginCommand"
[LoggingBehavior]        → Log: "[INFO] Processing LoginCommand for user 'john'"
[CachingBehavior]        → Skip (login is not cacheable)
[ValidationBehavior]     → Validate: username is not empty, password is at least 6 chars
[AuthorizationBehavior]  → Skip (login is a public endpoint)
[IdempotencyBehavior]    → Skip (login doesn't need idempotency)
[RetryBehavior]          → Stand by to retry on transient DB failures
[TimeoutBehavior]        → Set a maximum execution time
[TransactionBehavior]    → Begin database transaction

    ↓

STEP 4: Handler Executes Business Logic
LoginCommandHandler.Handle():

  1. Query database: SELECT * FROM "Users" WHERE "Username" = 'john'
  2. Verify password: BCrypt.Verify("SecurePass123!", storedHash)
  3. Generate JWT access token (15-minute expiry)
  4. Generate refresh token (7-day expiry)
  5. Save refresh token to database
  6. Return new LoginResult { AccessToken, RefreshToken, UserProfile }

    ↓

STEP 5: Pipeline Completes (Reverse Order)
[TransactionBehavior]    → COMMIT transaction
[TimeoutBehavior]        → Cancel timeout timer
[RetryBehavior]          → No retry needed
[PerformanceBehavior]    → Record: 45ms (OK — under 500ms threshold)
[MetricsBehavior]        → Log: "[METRIC] LoginCommand completed in 45ms"
[LoggingBehavior]        → Log: "[INFO] LoginCommand completed successfully"

    ↓

STEP 6: HTTP Response
200 OK

{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2g...",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john",
      "role": "Driver",
      "fullName": "John Doe"
    }
  },
  "errors": null,
  "timestamp": "2026-04-25T17:38:00Z"
}
```

---

> **Documentation Version:** 1.2  
> **Last Updated:** April 2026  
> **Repository:** https://github.com/Umar-Khattab/NYCTaxiData
