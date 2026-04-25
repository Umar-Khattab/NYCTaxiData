# Analytics Feature - CQRS Implementation Summary

This document summarizes what has been implemented in `NYCTaxiData.Application/Features/Analytics` using the project’s Clean Architecture + strict CQRS conventions.

## What Was Implemented

The Analytics module now contains four core sub-features:

1. **GetTopLevelKpisQuery**
2. **GetDemandVelocityChartQuery**
3. **GetSystemThresholdsQuery**
4. **UpdateSystemThresholdsCommand**

The implementation follows these standards:

- Request types are **records**
- Handlers use MediatR `IRequestHandler<TRequest, Result<TResponse>>`
- Input validation is handled with **FluentValidation** (input-only validation)
- Read models use query handlers and response DTOs
- Write operations use command handlers
- Marker interfaces are used:
  - `ICacheableQuery` for cacheable reads
  - `ITransactionalCommand` for transactional writes
- Standardized responses use `Result<T>`

---

## Folder Structure

`Features/Analytics/`

- `Common/`
  - `AnalyticsCacheKeys.cs`
- `Mappings/`
  - `AnalyticsMappingProfile.cs`
- `Queries/`
  - `GetTopLevelKpis/`
    - `GetTopLevelKpisQuery.cs`
    - `GetTopLevelKpisQueryHandler.cs`
  - `GetDemandVelocityChart/`
    - `GetDemandVelocityChartQuery.cs`
    - `GetDemandVelocityChartQueryHandler.cs`
    - `GetDemandVelocityChartQueryValidator.cs`
  - `GetSystemThresholds/`
    - `GetSystemThresholdsQuery.cs`
    - `GetSystemThresholdsQueryHandler.cs`
- `Commands/`
  - `UpdateSystemThresholds/`
    - `UpdateSystemThresholdsCommand.cs`
    - `UpdateSystemThresholdsCommandHandler.cs`
    - `UpdateSystemThresholdsCommandValidator.cs`

---

## Class-by-Class Summary

## `Common/AnalyticsCacheKeys.cs`

### `AnalyticsCacheKeys`
- Centralizes Redis/distributed cache keys used by Analytics.
- Currently includes a key for persisted system thresholds.

---

## `Queries/GetTopLevelKpis/GetTopLevelKpisQuery.cs`

### `GetTopLevelKpisQuery`
- Cacheable query (`ICacheableQuery`) for dashboard KPIs.
- Returns `Result<TopLevelKpisDto>`.

### `TopLevelKpisDto`
- DTO containing:
  - active drivers count
  - total daily revenue
  - average queue time (minutes)

## `Queries/GetTopLevelKpis/GetTopLevelKpisQueryHandler.cs`

### `GetTopLevelKpisQueryHandler`
- Aggregates KPI data from repositories via `IUnitOfWork`.
- Computes:
  - active fleet size
  - revenue for today
  - average queue/wait proxy from simulation metrics
- Returns successful `Result<TopLevelKpisDto>`.

---

## `Queries/GetDemandVelocityChart/GetDemandVelocityChartQuery.cs`

### `GetDemandVelocityChartQuery`
- Query for time-series demand velocity data.
- Supports optional `ZoneId` and configurable `Hours` window.
- Returns `Result<DemandVelocityChartDto>`.

### `DemandVelocityChartDto`
- Envelope DTO with range metadata and data points.

### `DemandVelocityPointDto`
- Single chart point containing time bucket + predicted values.

## `Queries/GetDemandVelocityChart/GetDemandVelocityChartQueryValidator.cs`

### `GetDemandVelocityChartQueryValidator`
- Validates input constraints only:
  - `Hours` in valid range
  - `ZoneId` positive when provided

## `Queries/GetDemandVelocityChart/GetDemandVelocityChartQueryHandler.cs`

### `GetDemandVelocityChartQueryHandler`
- Reads demand prediction rows for requested interval.
- Applies zone filtering when needed.
- Groups by time bucket and projects chart points.
- Returns `Result<DemandVelocityChartDto>`.

---

## `Queries/GetSystemThresholds/GetSystemThresholdsQuery.cs`

### `GetSystemThresholdsQuery`
- Cacheable query for current runtime thresholds.
- Returns `Result<SystemThresholdsDto>`.

### `SystemThresholdsDto`
- Response root DTO containing:
  - surge multiplier thresholds
  - dispatch radius thresholds
  - last updated timestamp

### `SurgeMultipliersDto`
- DTO for surge pricing thresholds (`Normal`, `Elevated`, `Critical`).

### `DispatchRadiusThresholdsDto`
- DTO for dispatch radius thresholds in KM (`Normal`, `Elevated`, `Critical`).

## `Queries/GetSystemThresholds/GetSystemThresholdsQueryHandler.cs`

### `GetSystemThresholdsQueryHandler`
- Reads thresholds from distributed cache first.
- Falls back to calculated defaults/derived values when cache is empty.
- Returns standardized `Result<SystemThresholdsDto>`.

---

## `Commands/UpdateSystemThresholds/UpdateSystemThresholdsCommand.cs`

### `UpdateSystemThresholdsCommand`
- Transactional command (`ITransactionalCommand`) to update thresholds.
- Returns `Result<UpdateSystemThresholdsResultDto>`.

### `UpdateSurgeMultipliersDto`
- Input DTO for surge multiplier values.

### `UpdateDispatchRadiusThresholdsDto`
- Input DTO for dispatch radius values.

### `UpdateSystemThresholdsResultDto`
- Output DTO confirming updated threshold values and timestamp.

## `Commands/UpdateSystemThresholds/UpdateSystemThresholdsCommandValidator.cs`

### `UpdateSystemThresholdsCommandValidator`
- Validates command inputs only.
- Enforces:
  - required payload sections
  - valid numeric ranges
  - ascending threshold order (`Normal < Elevated < Critical`)

## `Commands/UpdateSystemThresholds/UpdateSystemThresholdsCommandHandler.cs`

### `UpdateSystemThresholdsCommandHandler`
- Persists updated thresholds into distributed cache.
- Produces response payload with effective values and `UpdatedAtUtc`.
- Returns standardized success `Result<UpdateSystemThresholdsResultDto>`.

---

## `Mappings/AnalyticsMappingProfile.cs`

### `AnalyticsMappingProfile`
- Feature-specific AutoMapper profile for Analytics mappings.
- Defines system-threshold-related mapping configuration.
- Includes a note that KPI and chart responses are projected manually in handlers for performance.

---

## Notes

- Analytics controllers/endpoints should remain thin and call MediatR only.
- Any additional analytics query/command should follow the same pattern:
  - Request record
  - Validator (if input exists)
  - Handler returning `Result<T>`
  - DTOs co-located with feature
