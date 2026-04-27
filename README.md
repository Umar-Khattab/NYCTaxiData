# NYCTaxiData

Backend platform for NYC taxi operations built with **.NET 10**, **Clean Architecture**, **CQRS/MediatR**, **PostgreSQL**, and **SignalR**.

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Solution Structure](#solution-structure)
- [Core Functional Areas](#core-functional-areas)
- [Request Processing Pipeline (MediatR Behaviors)](#request-processing-pipeline-mediatr-behaviors)
- [API Reference](#api-reference)
- [SignalR Hubs](#signalr-hubs)
- [Configuration](#configuration)
- [Getting Started](#getting-started)
- [Build, Test, and CI](#build-test-and-ci)
- [Operational Notes](#operational-notes)

---

## Overview

NYCTaxiData provides backend services for:
- Driver and fleet management
- Trip lifecycle operations and dispatch
- Analytics and operational thresholds
- AI prediction and simulation endpoints
- Real-time manager/driver communication via SignalR
- Authentication and authorization with JWT

The codebase is organized around **feature-first CQRS** and enforces separation of concerns through layered architecture.

---

## Architecture

The project follows a Clean Architecture layering model:

- **Domain**: Entities, enums, and core contracts
- **Application**: CQRS requests/handlers, validators, mappings, pipeline behaviors, result models
- **Infrastructure**: EF Core persistence, repositories, unit of work, external integrations (Twilio, AI service), interceptors
- **API**: Controllers, hub endpoints, middleware, host configuration

Dependency direction is inward (API → Infrastructure/Application → Domain).

### Patterns and Practices

- **CQRS + MediatR** for command/query separation
- **Result pattern** for standardized success/failure handling
- **FluentValidation** for request validation
- **Repository + Unit of Work** abstractions
- **Specification-based querying** in repository operations
- **SignalR** for real-time updates
- **JWT Bearer authentication** for API and hub connections

---

## Solution Structure

```text
NYCTaxiData/
├── NYCTaxiData.API/              # REST + SignalR host
├── NYCTaxiData.Application/      # CQRS handlers, behaviors, validators, DTOs
├── NYCTaxiData.Domain/           # Entities, enums, domain interfaces
├── NYCTaxiData.Infrastructure/   # Data access, services, integrations
├── .github/workflows/dotnet.yml  # CI pipeline
└── README.md
```

---

## Core Functional Areas

### 1) Authentication & Identity
- Login
- Register driver / manager
- OTP send / verify
- Password reset
- Refresh token
- Profile retrieval

### 2) Drivers
- Driver list retrieval with filters
- Active fleet retrieval
- Driver profile and shift statistics
- Driver status update
- Offline data synchronization

### 3) Trips
- Start trip
- End trip
- Trip history (paginated)
- Online drivers retrieval
- Live dispatch feed
- Manual dispatch
- Audit/testing endpoints (for interceptor behavior testing)

### 4) Analytics
- Top-level KPI retrieval
- Demand velocity chart data
- System thresholds retrieval/update

### 5) AI / Simulation
- Demand prediction (15-minute, 6-hour)
- ETA prediction
- Revenue and stock-out prediction
- Profit-zone ranking
- Causal impact estimation
- Repositioning optimization
- Fleet expansion simulation start and retrieval

> Note: `ZonesController` is currently commented out in source, so zone REST endpoints are intentionally inactive in the current host configuration. Treat zones as planned/incomplete API surface unless this controller is re-enabled.

---

## Request Processing Pipeline (MediatR Behaviors)

Application DI registers behaviors in this order:

1. `ExceptionHandlingBehavior`
2. `MetricsBehavior`
3. `PerformanceBehavior`
4. `LoggingBehavior`
5. `AuthorizationBehavior`
6. `ValidationBehavior`
7. `CachingBehavior`
8. `IdempotencyBehavior`
9. `RetryBehavior`
10. `TimeoutBehavior`
11. `TransactionBehavior`

This provides consistent handling for validation, authorization, observability, resilience, and transactional safety around request handlers.

---

## API Reference

Base route pattern: `api/v1/{controller}` unless explicitly set (AI uses `api/v1/ai`).

### Auth (`/api/v1/auth`)
- `POST /login`
- `POST /register/driver`
- `POST /register/manager`
- `POST /otp/send`
- `POST /otp/verify`
- `POST /password/reset`
- `POST /token/refresh`
- `GET /profile/{phoneNumber}`

### Drivers (`/api/v1/drivers`)
- `GET /`
- `GET /active`
- `GET /{driverId}`
- `GET /{driverId}/shift-stats`
- `PUT /{driverId}/status`
- `POST /sync-offline`

### Trips (`/api/v1/trips`)
- `POST /start`
- `POST /end`
- `GET /`
- `GET /online`
- `GET /live-dispatch`
- `POST /dispatch`
- `POST /test-audit`
- `DELETE /{id}`

### Analytics (`/api/v1/analytics`) *(Authorize: Admin, Dispatcher)*
- `GET /kpis`
- `GET /demand-velocity`
- `GET /thresholds`
- `PUT /thresholds`

### AI (`/api/v1/ai`)
- `POST /predict/demand-15min`
- `POST /predict/demand-6h`
- `POST /predict/eta`
- `POST /predict/revenue`
- `POST /predict/stockout`
- `POST /predict/profit-zones`
- `POST /predict/causal-impact`
- `POST /optimize/repositioning`
- `POST /simulate/fleet-expansion`
- `GET /simulate/{simulationId}?pageNumber=1&pageSize=10`

---

## SignalR Hubs

Configured endpoints:
- `/hubs/taxi`
- `/hubs/tracking`
- `/hubs/dispatch`

### Authentication for Hubs
JWT is accepted from query string `access_token` for `/hubs/*` paths via `SignalRJwtExtension`.

### Hub Capabilities (high level)
- `TaxiHub`: role-based groups, driver location broadcast, dispatch command push, trip status updates
- `LiveTrackingHub`: live location updates, active drivers snapshot, status updates
- `DispatchHub`: manager/driver grouping, dispatch accept/reject events, pickup/trip-completion events

---

## Configuration

Primary settings are under `NYCTaxiData.API/appsettings*.json`.

### Required keys

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=...;Username=...;Password=..."
  },
  "Jwt": {
    "Secret": "<min-32-char-secret>",
    "Issuer": "NYCTaxiData",
    "Audience": "NYCTaxiData"
  },
  "MediatR": {
    "LicenseKey": "<optional-host-config-value>"
  },
  "AutoMapper": {
    "LicenseKey": "<optional-host-config-value>"
  },
  "MlService": {
    "BaseUrl": "https://your-ml-service"
  }
}
```

`MediatR:LicenseKey` and `AutoMapper:LicenseKey` are optional host configuration entries currently read by DI registration in this codebase; standard usage of these libraries does not require adding paid licenses for this project setup.

### Security guidance
- Do **not** commit real credentials/secrets.
- Prefer environment variables or user secrets for local development.
- Rotate any leaked or shared secrets immediately.

---

## Getting Started

1. Install prerequisites:
   - .NET SDK 10
   - PostgreSQL
2. Configure `appsettings.Development.json` (or secrets/env vars).
3. From repository root:

```bash
dotnet restore
dotnet build --no-restore
```

4. Run API:

```bash
cd NYCTaxiData.API
dotnet run
```

Default local URLs from `NYCTaxiData.API/Properties/launchSettings.json` (verify if you changed launch profiles):
- `http://localhost:5006`
- `https://localhost:7112`

---

## Build, Test, and CI

CI workflow (`.github/workflows/dotnet.yml`) executes:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build --verbosity normal
```

Use the same commands locally before pushing changes.

---

## Operational Notes

- Global exception handling is configured via `GlobalExceptionHandler` middleware.
- Application and infrastructure services are registered through layer-specific DI extension methods.
- The API host currently includes some direct registrations in `Program.cs` in addition to layer DI modules; keep service registration consistent when extending the project.
