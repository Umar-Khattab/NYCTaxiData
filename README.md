# NYC Taxi Data — Technical Documentation

> **Repository**: https://github.com/Umar-Khattab/NYCTaxiData  
> **Framework**: .NET 10  
> **Last Updated**: April 26, 2026  
> **Document Version**: 1.4

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture Overview](#2-architecture-overview)
3. [Project Structure](#3-project-structure)
4. [Code Flow](#4-code-flow)
5. [Branch Analysis](#5-branch-analysis)
6. [Key Components Explanation](#6-key-components-explanation)
7. [Configuration & Setup](#7-configuration--setup)
8. [Dependencies & Integrations](#8-dependencies--integrations)
9. [Best Practices & Observations](#9-best-practices--observations)

---

## 1. Project Overview

### Purpose of the System

The **NYCTaxiData** project is a comprehensive backend system for managing NYC taxi operations. It serves as a centralized platform that orchestrates:

- **Fleet Management**: Real-time driver status tracking, shift statistics, and active fleet monitoring
- **Trip Dispatch**: Manual and AI-assisted trip assignment, live dispatch feeds, and trip lifecycle management
- **Analytics & KPIs**: System thresholds, demand velocity charts, and top-level performance metrics
- **AI/ML Intelligence**: Demand forecasting, dispatch recommendations, optimal driver scheduling, revenue prediction, and operational simulations
- **Authentication & Authorization**: Multi-method auth (password, OTP, OAuth, SAML, WebAuthn) with role-based access control
- **Real-time Communication**: SignalR hubs for live tracking and dispatch notifications

### Main Features

| Feature Domain | Capabilities |
|----------------|-------------|
| **Authentication** | Login/Register (Driver/Manager), OTP via WhatsApp (Twilio), Password Reset, Refresh Tokens, Role-based access |
| **Driver Management** | Active fleet queries, driver profiles, shift statistics, offline data sync, status updates |
| **Trip Management** | Start/End trips, manual dispatch, live dispatch feed, trip history with pagination |
| **Zone Management** | Zone listings, live demand heatmaps, zone-specific insights |
| **Analytics** | Top-level KPIs, system thresholds configuration, demand velocity visualization |
| **AI/ML** | Demand forecasting (15min/6h), ETA prediction, revenue prediction, stock-out prediction, causal impact estimation, zone profit ranking, fleet expansion simulation, voice assistant, model retraining |
| **Real-time** | SignalR hubs for dispatch notifications and live driver tracking |
| **Performance** | 11 MediatR pipeline behaviors for monitoring, caching, validation, authorization, idempotency, retry, timeout, transactions |

### Technologies Used

- **.NET 10** — Primary framework
- **Entity Framework Core** — ORM with PostgreSQL provider (Npgsql)
- **MediatR** — CQRS and mediator pattern implementation
- **FluentValidation** — Request validation
- **AutoMapper** — Object-to-object mapping
- **SignalR** — Real-time bidirectional communication
- **Twilio** — WhatsApp SMS/OTP integration
- **JWT** — Token-based authentication
- **xUnit/MSTest** — Testing framework (implied by test project structure)

---

## 2. Architecture Overview

### Architecture Style: Clean Architecture

The project strictly follows **Clean Architecture** principles with four distinct layers:

```
┌─────────────────────────────────────┐
│  Presentation Layer (API)         │  ← Controllers, Hubs, Middleware
├─────────────────────────────────────┤
│  Application Layer                │  ← CQRS Handlers, Behaviors, DTOs
├─────────────────────────────────────┤
│  Infrastructure Layer             │  ← DbContext, Repositories, Services
├─────────────────────────────────────┤
│  Domain Layer                     │  ← Entities, Interfaces, Enums
└─────────────────────────────────────┘
```

**Dependency Direction**: Domain → Application → Infrastructure → API (inward-only dependencies)

### Layer Responsibilities

| Layer | Responsibility |
|-------|---------------|
| **Domain** | Core business entities, value objects, domain interfaces, enums. No external dependencies. |
| **Application** | Business logic orchestration via CQRS handlers, DTOs, validation rules, pipeline behaviors, mapping profiles. Depends only on Domain. |
| **Infrastructure** | Data access (EF Core, repositories), external service integrations (Twilio, JWT, AI prediction), interceptors. Depends on Domain and Application. |
| **API** | HTTP endpoints (REST controllers), SignalR hubs, middleware, DI configuration. Depends on all inner layers. |

### Design Patterns Used

1. **CQRS (Command Query Responsibility Segregation)** — Separates read (Queries) and write (Commands) operations via MediatR
2. **Repository Pattern** — `IGenericRepository<T>` abstracts data access; `GenericRepository<T>` implements EF Core queries
3. **Unit of Work** — `IUnitOfWork` coordinates multiple repository operations within a single transaction
4. **Specification Pattern** — `ISpecification<T>` encapsulates query criteria (e.g., `UserByPhoneSpec`, `ActiveTripsSpec`)
5. **Pipeline Behaviors (Middleware Chain)** — 11 cross-cutting MediatR behaviors wrap every request
6. **Dependency Injection** — Constructor injection throughout; service registration in `Program.cs` and `DependencyInjection.cs`
7. **Marker Interfaces** — `ICacheableQuery`, `IIdempotentCommand`, `ITransactionalCommand`, `ISecureRequest` for declarative behavior configuration
8. **Result Pattern** — `Result<T>` and `Error` classes for functional error handling

---

## 3. Project Structure

### Solution Layout

```
NYCTaxiData/
├── NYCTaxiData.Domain/              # Core business logic
│   ├── Entities/                    # 30+ domain models
│   ├── Enums/                       # Status, Role, RiskLevel, etc.
│   ├── Interfaces/                  # IGenericRepository, IUnitOfWork, ISpecifications
│   └── DTOs/                        # Common DTOs (DemandPredictionDtos, etc.)
│
├── NYCTaxiData.Application/         # Business logic orchestration
│   ├── Behaviors/                   # 11 pipeline behaviors + extensive README docs
│   ├── Common/
│   │   ├── Interfaces/              # IApplicationDbContext, ICurrentUserService, IAiPredictionService
│   │   ├── Specifications/            # Query specs (Auth, Trips, Managers)
│   │   ├── Mappings/                # AutoMapper profiles
│   │   ├── Exceptions/              # ValidationException, NotFoundException, etc.
│   │   └── Models/                  # PaginatedList, PaginationParams, Result/Error
│   ├── DTOs/
│   │   ├── Identity/                # LoginDto, RegisterDto, OTP DTOs
│   │   ├── Trip/                    # Trip result DTOs
│   │   └── Tracking/                # Dispatch/location DTOs
│   ├── Features/                    # CQRS features organized by domain
│   │   ├── Auth/                    # Login, Register, OTP, ResetPassword, RefreshToken
│   │   ├── Drivers/                 # UpdateStatus, SyncOfflineData, GetActiveFleet, GetShiftStatistics
│   │   ├── Trips/                   # StartTrip, EndTrip, ManualDispatch, GetLiveDispatchFeed, GetTripHistory
│   │   ├── Zones/                   # GetAllZones, GetLiveDemandHeatmap, GetSpecificZoneInsights
│   │   ├── Analytics/               # GetTopLevelKpis, GetSystemThresholds, GetDemandVelocityChart, UpdateSystemThresholds
│   │   └── AI/                      # 15+ AI commands and queries
│   └── DependencyInjection.cs       # Service registration (MediatR, AutoMapper, FluentValidation, Behaviors)
│
├── NYCTaxiData.Infrastructure/        # Data access & external services
│   ├── Data/
│   │   ├── Contexts/TaxiDbContext.cs # EF Core DbContext (~78KB, all entity mappings)
│   │   ├── Repository/GenericRepository.cs
│   │   └── Initializers/            # Db seeding
│   ├── Interceptors/                # Audit logging, auditable entity tracking, current user service
│   ├── Services/
│   │   ├── Specifications/          # Infrastructure-level specs + SpecificationEvaluator
│   │   ├── Twilio/                  # WhatsAppSmsService, TwilioSettings
│   │   ├── AiPredictionService.cs   # ML prediction service implementation
│   │   ├── CacheService.cs          # In-memory caching
│   │   ├── JwtTokenService.cs       # JWT generation
│   │   └── UnitOfWork.cs            # Transaction coordination
│   └── DependencyInjection.cs       # Infrastructure service registration
│
├── NYCTaxiData.API/                 # REST API & real-time layer
│   ├── Controllers/                 # Auth, Drivers, Trips, Analytics, AI, Zones
│   │   └── Base/BaseController.cs   # Common controller functionality
│   ├── Hups/                        # SignalR hubs
│   │   ├── Dispatch/DispatchHub.cs  # Dispatch notifications hub
│   │   ├── LiveTrackingHub.cs       # Real-time driver tracking
│   │   └── TaxiHub.cs               # General taxi operations hub
│   ├── MiddleWares/
│   │   └── GlobalExceptionHandler.cs
│   ├── Extensions/
│   │   ├── QueryableExtensions.cs   # Pagination helpers
│   │   └── SignalRJwtExtension.cs   # JWT auth for SignalR
│   ├── Contracts/APIResponse.cs     # Standard API response wrapper
│   └── Program.cs                   # App configuration & DI setup
│
├── .github/workflows/dotnet.yml     # CI/CD pipeline
├── .postman/                        # Postman configuration
├── postman/                         # Postman globals
└── README.md                        # Project README (45KB, very detailed)
```

### Key File Sizes (Indicators of Complexity)

| File | Size | Significance |
|------|------|-------------|
| `TaxiDbContext.cs` | ~78 KB | Large EF Core context with 30+ entity mappings |
| `PerformanceBehavior.cs` | ~12.6 KB | Real-time performance monitoring with rolling windows |
| `CachingBehavior.cs` | ~9 KB | Sophisticated response caching logic |
| `IdempotencyBehavior.cs` | ~8.5 KB | Duplicate request detection and handling |
| `MetricsBehavior.cs` | ~9.2 KB | Metrics collection infrastructure |
| `ExceptionHandlingBehavior.cs` | ~7.3 KB | Global exception wrapping |
| `RetryBehavior.cs` | ~7.4 KB | Automatic retry with backoff |
| `README.md` | ~46 KB | Extensive project documentation |
| `TRIPS_CQRS_README.md` | ~18 KB | Detailed trips module documentation |

---

## 4. Code Flow

### End-to-End Request Lifecycle

A typical request (e.g., `POST /api/auth/login`) flows through the system as follows:

```
1. HTTP Request → API Controller (AuthController)
   ↓
2. Controller creates LoginCommand and calls _sender.Send(command)
   ↓
3. MediatR Pipeline Behaviors execute in order:

   [1] MetricsBehavior        → Start timing, collect request metrics
   [2] PerformanceBehavior      → Monitor for slow operations (>500ms queries, >1000ms commands)
   [3] LoggingBehavior          → Log request/response details
   [4] CachingBehavior          → Check cache (skip for commands, use for cacheable queries)
   [5] ValidationBehavior       → Run FluentValidation rules
   [6] AuthorizationBehavior    → Check ISecureRequest marker, verify permissions
   [7] IdempotencyBehavior      → Check IIdempotentCommand, prevent duplicates
   [8] RetryBehavior            → Retry on transient failures (max 3 attempts)
   [9] TimeoutBehavior          → Enforce operation timeouts
   [10] TransactionBehavior     → Begin DB transaction for ITransactionalCommand
   ↓
4. Handler Execution (LoginCommandHandler)
   - Uses IUnitOfWork / IGenericRepository to query User entity
   - Verifies password via domain logic
   - Generates JWT tokens via IJwtTokenService
   - Returns Result<LoginResponse>
   ↓
5. TransactionBehavior commits (if successful)
   ↓
6. PerformanceBehavior checks thresholds, logs if degraded
   ↓
7. Response flows back through behaviors
   ↓
8. Controller wraps in APIResponse<T> and returns HTTP 200
```

### Interaction Between Layers

```
API Controller
    ↓ (calls)
Application Handler (CQRS)
    ↓ (uses)
Application Interfaces (IUnitOfWork, IGenericRepository, IJwtTokenService)
    ↓ (implemented by)
Infrastructure Services (UnitOfWork, GenericRepository, JwtTokenService)
    ↓ (uses)
Domain Entities (User, Driver, Trip, etc.)
    ↓ (persisted via)
EF Core + PostgreSQL
```

---

## 5. Branch Analysis

### Branch Inventory

| Branch | Status | Relative to Master | Last Activity |
|--------|--------|-------------------|---------------|
| `master` | Default | — | Apr 26, 2026 |
| `AI-Changes` | Active / Diverged | 1 commit ahead | Apr 26, 2026 |
| `Spec_Repo_Auht` | Stale | 40 commits behind | Apr 12, 2026 |
| `feat/identity-uow-impl` | Stale | 79 commits behind | Apr 9, 2026 |
| `infra-signalr-plumbing` | Merged (PR #4) | 18 commits behind | Apr 25, 2026 |
| `copilot/add-detailed-readme-file` | Merged | Same as master | Apr 26, 2026 |

---

### 5.1 Branch: `master` (Default)

**Purpose**: Primary development and production branch

**Current State**: Contains all merged features including AI module, SignalR infrastructure, and full CQRS implementation.

**Recent Evolution (last 20 commits)**:
- **Apr 26**: Merged PR #5 (`AI-Changes`) — AI controller and features
- **Apr 26**: Service interface refactoring — all services now implement proper interfaces for Application layer consumption
- **Apr 26**: Controller standardization — replaced `IMediator` with `ISender` for cleaner dependency injection
- **Apr 25**: Merged PR #4 (`infra-signalr-plumbing`) — SignalR hubs and real-time infrastructure
- **Apr 25**: README update with comprehensive documentation

---

### 5.2 Branch: `AI-Changes`

**Purpose**: Feature branch for AI/ML controller and service integration

**Status**: Diverged from master (1 commit ahead). The branch contains a commit that deletes `COMPLETE_PROJECT_CONTEXT.md` — this appears to be a cleanup commit after the AI features were merged into master via PR #5.

**Key Changes Compared to Master**:
- Removal of `COMPLETE_PROJECT_CONTEXT.md` (a large documentation file that was previously in the repo)
- The AI controller implementation and service registrations were already merged into master via PR #5

**Impact**: Minimal architectural impact — primarily documentation cleanup.

---

### 5.3 Branch: `Spec_Repo_Auht`

**Purpose**: Implementation of Specification Pattern, Generic Repository, and Unit of Work

**Status**: 40 commits behind master (stale). Last commit: "implement specification pattern, generic repository, and unit of work" (Apr 12, 2026, by Mohammedyassin22).

**Key Changes Compared to Master**:
- Introduced `ISpecification<T>` interface and `BaseSpecification<T>` implementation
- Added `SpecificationEvaluator` to translate specs into EF Core queries
- Created domain-level specs: `UserByPhoneSpec`, `UserForLoginSpec`, `TripHistorySpec`, etc.
- Established `IGenericRepository<T>` with `GetAsync(ISpecification)` overloads
- Implemented `UnitOfWork` for transaction management

**Impact on Architecture**: This branch laid the foundational data access patterns that were subsequently merged into master. The specification pattern enables type-safe, composable query criteria without exposing IQueryable outside the infrastructure layer.

**Notable**: This branch appears to have been superseded by master — its patterns were integrated but the branch itself was not deleted.

---

### 5.4 Branch: `feat/identity-uow-impl`

**Purpose**: Authentication flow and repository pattern implementation

**Status**: 79 commits behind master (very stale). Last commit: "feat: implement auth flow and repository patterns" (Apr 9, 2026, by Mohammedyassin22).

**Key Changes Compared to Master**:
- Initial authentication commands: Login, Register, OTP
- Basic repository pattern setup
- Identity-related DTOs and validators
- Early Unit of Work implementation

**Impact on Architecture**: This was the **foundational branch** for the entire identity system. All auth features in master (Login, Register, OTP, Password Reset, Refresh Token) trace their origin to this branch.

**Notable**: Being 79 commits behind indicates this branch was merged early and development continued on other branches. It represents the project's initial architectural bootstrap.

---

### 5.5 Branch: `infra-signalr-plumbing`

**Purpose**: Real-time communication infrastructure via SignalR

**Status**: Merged into master via PR #4 (Apr 25, 2026). Currently 18 commits behind master.

**Key Changes Compared to Pre-Merge Master**:
- **SignalR Hubs**:
  - `DispatchHub` — Real-time dispatch notifications with group-based routing
  - `LiveTrackingHub` — Driver location tracking with JWT authentication
  - `TaxiHub` — General taxi operations hub
- **JWT SignalR Extension** (`SignalRJwtExtension.cs`) — Enables JWT token validation for WebSocket connections
- **Dispatch Notification Service** — `IDispatchNotificationService` interface for sending real-time alerts
- **AI Dispatch Order DTO** — Structured data for AI-driven dispatch recommendations delivered via SignalR

**Impact on Architecture**: Transformed the system from a purely request-response API into a real-time platform. The SignalR infrastructure enables:
- Live driver tracking on maps
- Instant dispatch notifications to drivers
- Real-time demand heatmap updates
- AI recommendation push delivery

**Merge Resolution**: The final commit on this branch ("fix: resolve all merge conflicts and finalize infrastructure plumbing") indicates significant merge conflict resolution, suggesting parallel development with other feature branches.

---

### 5.6 Branch: `copilot/add-detailed-readme-file`

**Purpose**: Documentation enhancement via GitHub Copilot

**Status**: Merged into master. Currently identical to master (same HEAD commit).

**Key Changes**: Added comprehensive `README.md` (45KB) with detailed project description, architecture diagrams, and setup instructions.

---

## 🔀 Cross-Branch Insights

### How the Project Evolved

The repository shows a clear **branch-per-feature** workflow with the following evolutionary timeline:

```
Apr 9  → feat/identity-uow-impl     [Foundation: Auth + Repositories]
   ↓
Apr 12 → Spec_Repo_Auht            [Pattern: Specifications + UoW]
   ↓
Apr 25 → infra-signalr-plumbing     [Real-time: SignalR Hubs]
   ↓        ↓
   └────────┘→ master (merge PR #4)
   ↓
Apr 26 → AI-Changes                 [Intelligence: AI/ML Features]
   ↓
        → master (merge PR #5)
```

### Major Refactors

1. **Controller Standardization** (Apr 26): All controllers migrated from `IMediator` to `ISender` — a more focused interface that reduces coupling to MediatR internals.

2. **Service Interface Extraction** (Apr 26): Infrastructure services (JWT, Cache, AI Prediction) had their interfaces moved to the Application layer (`Common/Interfaces/Services/`), enabling proper dependency inversion. Previously, Application handlers likely depended directly on Infrastructure implementations.

3. **Behavior Pipeline Completion**: The project evolved from basic CQRS to a comprehensive 11-behavior pipeline, with each behavior documented by its own README, quick reference, and implementation summary.

### Architectural Shifts

| Phase | Branch | Shift |
|-------|--------|-------|
| Foundation | `feat/identity-uow-impl` | From monolithic to layered architecture |
| Data Access | `Spec_Repo_Auht` | From direct DbContext to Repository + Specification patterns |
| Real-time | `infra-signalr-plumbing` | From REST-only to REST + WebSocket hybrid |
| Intelligence | `AI-Changes` | From CRUD to AI-augmented decision making |

### Patterns Introduced or Removed

**Introduced**:
- Specification Pattern (via `Spec_Repo_Auht`)
- Marker Interfaces for declarative behavior (`ICacheableQuery`, `IIdempotentCommand`, etc.)
- Result/Error functional pattern (replacing exception-based flow in some areas)
- SignalR JWT authentication extension
- AI prediction service abstraction (`IAiPredictionService`)

**Removed/Deprecated**:
- `COMPLETE_PROJECT_CONTEXT.md` (removed in `AI-Changes` branch after being merged)
- Direct `IMediator` usage in controllers (replaced with `ISender`)

---

## 6. Key Components Explanation

### Services

| Service | Layer | Role |
|---------|-------|------|
| **AuthService** | Infrastructure | Orchestrates login, registration, OTP verification, token generation. Implements domain auth logic. |
| **JwtTokenService** | Infrastructure | Generates and validates JWT access/refresh tokens. |
| **CacheService** | Infrastructure | In-memory caching for frequently accessed data (e.g., analytics KPIs). |
| **WhatsAppSmsService** | Infrastructure | Twilio integration for OTP delivery via WhatsApp. |
| **AiPredictionService** | Infrastructure | ML model invocation for demand forecasting, ETA prediction, revenue optimization. |
| **UnitOfWork** | Infrastructure | Coordinates `SaveChangesAsync()` across multiple repositories within a transaction scope. |
| **CurrentUserService** | Infrastructure | Extracts user identity from HTTP context for audit logging. |
| **DispatchNotificationService** | Infrastructure | Sends real-time notifications via SignalR to drivers and dispatchers. |

### Interfaces

| Interface | Purpose |
|-----------|---------|
| `IGenericRepository<T>` | CRUD operations + specification-based queries. Decouples Application from EF Core. |
| `IUnitOfWork` | Transaction boundary abstraction. Ensures atomic operations across multiple aggregates. |
| `ISpecification<T>` | Encapsulates query criteria (Where, Include, OrderBy) as reusable objects. |
| `IApplicationDbContext` | Abstraction over `DbContext` for unit testing and loose coupling. |
| `ICurrentUserService` | Provides current user ID/name without direct HTTP context dependency in handlers. |
| `IAiPredictionService` | Gateway to ML models — allows swapping prediction backends without handler changes. |
| `IIdempotencyService` | Checks and stores idempotency keys to prevent duplicate command execution. |
| `IDispatchNotificationService` | Abstraction for real-time notification delivery (SignalR, push, SMS). |
| **Marker Interfaces** | |
| `ICacheableQuery` | Marks a query for CachingBehavior processing |
| `IIdempotentCommand` | Marks a command for IdempotencyBehavior processing |
| `ITransactionalCommand` | Marks a command for TransactionBehavior processing |
| `ISecureRequest` | Marks a request for AuthorizationBehavior processing |

### Repositories

- **GenericRepository<T>** — Base implementation using EF Core. Supports:
  - `GetAllAsync()`, `GetByIdAsync()`
  - `GetAsync(ISpecification<T>)` — specification-driven queries
  - `AddAsync()`, `Update()`, `Delete()`
  - `CountAsync()`, `AnyAsync()`

### Specifications

Specifications encapsulate query logic and enable composition:

| Specification | Criteria |
|--------------|----------|
| `UserByPhoneSpec` | Filter users by phone number |
| `UserForLoginSpec` | Include roles/claims for login query |
| `ActiveTripsSpec` | Filter trips with "Active" status |
| `TripHistorySpec` | Filter by driver + date range + pagination |
| `AvailableDriversSpec` | Filter drivers with "Available" status |
| `DispatchFeedSpec` | Filter pending trips with zone/driver includes |

### Middleware

- **GlobalExceptionHandler** — Catches unhandled exceptions, logs them, and returns standardized `APIResponse` with appropriate HTTP status codes. Integrates with the ExceptionHandlingBehavior for consistent error formatting.

---

## 7. Configuration & Setup

### Clone the Repository

```bash
git clone https://github.com/Umar-Khattab/NYCTaxiData.git
cd NYCTaxiData
```

### Required Tools

- .NET 10 SDK
- PostgreSQL 14+ (or compatible)
- Twilio account (for WhatsApp OTP)
- Optional: Postman (collection configuration included in `.postman/`)

### Environment Setup

1. **Database Configuration**:
   Update `NYCTaxiData.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=NYCTaxiData;Username=postgres;Password=your_password"
     }
   }
   ```

2. **Twilio Configuration** (for OTP):
   Update `NYCTaxiData.Infrastructure/Services/Twilio/TwilioSettings` or use UserSecrets:
   ```json
   {
     "Twilio": {
       "AccountSid": "your_account_sid",
       "AuthToken": "your_auth_token",
       "FromNumber": "your_twilio_number"
     }
   }
   ```

3. **JWT Configuration**:
   Add to `appsettings.json`:
   ```json
   {
     "Jwt": {
       "Key": "your_super_secret_key_min_32_chars",
       "Issuer": "NYCTaxiData",
       "Audience": "NYCTaxiData.Client",
       "ExpiryMinutes": 60
     }
   }
   ```

### Run the Project

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run database migrations (if using EF Core migrations)
dotnet ef database update --project NYCTaxiData.Infrastructure --startup-project NYCTaxiData.API

# Or use the initializer (DbInitializers.cs seeds data on first run)

# Run the API
cd NYCTaxiData.API
dotnet run

# API will be available at:
# https://localhost:5000 (production)
# https://localhost:5001 (development)
```

### Verify Setup

- Swagger UI: `https://localhost:5000/swagger`
- SignalR hubs: `wss://localhost:5000/hubs/dispatch`
- Health check: `GET /api/health` (if configured)

---

## 8. Dependencies & Integrations

### Databases

- **PostgreSQL** — Primary relational database via Npgsql EF Core provider
- **Connection Resilience** — Configured with retry policy (3 retries, 5-second delays)
- **Schema Migrations** — Tracked via `SchemaMigration` entity; initialization via `IDbInitializer`

### External APIs

| Service | Integration Point | Purpose |
|---------|-------------------|---------|
| **Twilio** | `WhatsAppSmsService` | OTP delivery via WhatsApp |
| **ML Model** | `AiPredictionService` | Demand forecasting, ETA, revenue prediction |
| **JWT Provider** | `JwtTokenService` | Token generation/validation (internal) |

### Third-Party Services

- **SignalR** — Real-time communication for dispatch and tracking
- **FluentValidation** — Request validation (11+ validators across features)
- **AutoMapper** — Entity-to-DTO mapping (4+ mapping profiles)
- **MediatR** — CQRS mediator and pipeline behaviors

---

## 9. Best Practices & Observations

### Good Design Decisions

1. **Comprehensive Pipeline Behaviors** — The 11-behavior MediatR pipeline is exceptionally thorough. Each behavior is well-documented with its own README, quick reference, and implementation summary. This demonstrates production-grade concern separation.

2. **Specification Pattern** — Query logic is encapsulated in testable, reusable specification classes rather than scattered in handlers or repositories.

3. **Marker Interfaces** — Declarative behavior configuration (`ICacheableQuery`, `IIdempotentCommand`) allows behaviors to self-configure without explicit registration logic.

4. **Result Pattern** — `Result<T>` and `Error` classes provide explicit error handling without exception throwing for business logic failures.

5. **Interface Segregation** — Application-layer interfaces (`IApplicationDbContext`, `ICurrentUserService`) prevent direct infrastructure dependencies in handlers.

6. **SignalR JWT Integration** — The `SignalRJwtExtension.cs` properly authenticates WebSocket connections using the same JWT scheme as HTTP APIs.

7. **Audit Interceptors** — `AuditLogInterceptor` and `AuditableEntityInterceptor` automatically track entity changes and user attribution.

### Code Quality Observations

| Observation | Severity | Details |
|-------------|----------|---------|
| Typo in folder name | Minor | `RegisrerManager` instead of `RegisterManager` in `Features/Auth/Commands/` |
| Duplicate `Result.cs` | Minor | Both `Common/Result.cs` and `Common/Plumping/Result.cs` exist — potential confusion |
| Duplicate `PaginatedList.cs` | Minor | Both `Common/PaginatedList.cs` and `Common/Models/PaginatedList.cs` exist |
| Empty AI handlers | Medium | Several AI command handlers (`ProcessVoiceAssistant`, `RunOperationalSimulation`, etc.) appear to be stubs (~220 bytes each) |
| `User1.cs` entity | Minor | Appears to be a duplicate or alternate user entity — unclear purpose |
| `SchemaMigration1.cs` | Minor | Duplicate migration tracking entity |
| `APIResponse.cs` in Controllers | Minor | Duplicate file in `Controllers/APIResponse.cs` (88 bytes) alongside `Contracts/APIResponse.cs` |
| `DependencyInjection.cs` history | Note | Was previously commented out; now fully implemented |

### Suggested Improvements

1. **Consolidate Duplicates** — Merge duplicate `Result.cs`, `PaginatedList.cs`, and `APIResponse.cs` files to reduce maintenance overhead.

2. **Complete AI Stubs** — The AI feature has excellent DTOs, enums, and validators, but several command handlers are empty stubs. These should be implemented or removed to avoid confusion.

3. **Behavior Registration Order** — Verify the pipeline behavior order in `DependencyInjection.cs`. The current order (Metrics → Performance → Logging → Caching → Validation → Authorization → Idempotency → Retry → Timeout → Transaction) is logical, but caching before validation means invalid requests may be cached.

4. **SignalR Scale-Out** — Current SignalR uses in-memory backplane. For multi-instance deployment, consider Redis backplane.

5. **Unit Tests** — The repository structure suggests a test project exists, but no test files were visible in the tree. Ensure comprehensive coverage for handlers and behaviors.

6. **Database Seeding** — `DbInitializers.cs` provides seeding, but consider using EF Core migrations for schema evolution instead of `SchemaMigration` entity tracking.

7. **Folder Naming** — Fix `RegisrerManager` typo and clarify `User1.cs` / `SchemaMigration1.cs` purpose.

8. **Documentation Sync** — `COMPLETE_PROJECT_CONTEXT.md` was deleted in the `AI-Changes` branch but may still be referenced. Ensure all documentation is consolidated in `README.md`.

---

## Appendix: Behavior Pipeline Documentation

The project includes extensive self-documentation for each pipeline behavior:

| Behavior | README | Quick Ref | Implementation Summary |
|----------|--------|-----------|----------------------|
| Authorization | ✅ | ✅ | ✅ |
| Caching | ✅ | ✅ | ✅ |
| Exception Handling | ✅ | ✅ | ✅ |
| Idempotency | ✅ | ✅ | ✅ |
| Logging | ✅ | ✅ | ✅ |
| Metrics | ✅ | ✅ | ✅ |
| Performance | ✅ | ✅ | ✅ |
| Retry | ✅ | ✅ | ✅ |
| Timeout | ✅ | ✅ | ✅ |
| Transaction | ✅ | ✅ | ✅ |
| Validation | ✅ | ✅ | ✅ |
| **Complete Report** | `ELEVEN_BEHAVIORS_COMPLETE.md` (17.8 KB) | | |

*End of Documentation*
