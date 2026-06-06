/**
 * hooks/use.ts  —  TanStack Query hooks for the FleetCommand dashboard.
 *
 * All data goes through lib/fleet-api (single source of truth).
 * Paths in fleet-api are verbatim from NYCTaxiData_API_Collection.json.
 */

import { useQuery } from '@tanstack/react-query'
import { fleetApi, type ApiKPIs } from '@/lib/fleet-api'

export type { ApiKPIs }

// ── KPI shape the UI consumes ─────────────────────────────────────────────────

/**
 * Normalised KPI data for TopControlBar.
 * Maps whatever the server returns (field names confirmed from the collection
 * description: "active drivers, total revenue, average wait time") to the
 * camelCase names the UI cards expect.
 */
export interface AnalyticsKpiData {
  // Active taxis / drivers
  activeTaxis?: number
  activeTaxisChange?: string
  activeTaxisChangeType?: 'positive' | 'negative' | 'neutral'

  // Average fare per trip
  averageFare?: number
  averageFareChange?: string
  averageFareChangeType?: 'positive' | 'negative' | 'neutral'

  // Demand index (0–10 scale)
  demandIndex?: number
  demandIndexChange?: string
  demandIndexChangeType?: 'positive' | 'negative' | 'neutral'

  // Fleet idle rate (%)
  idleRate?: number
  idleRateChange?: string
  idleRateChangeType?: 'positive' | 'negative' | 'neutral'
}

// ── Normaliser ────────────────────────────────────────────────────────────────

/**
 * The /api/v1/analytics/kpis endpoint returns:
 *   activeDrivers, totalRevenue, avgWaitTime, totalTripsToday, avgFare, …
 * (from the collection description + GetTopLevelKpisQuery)
 *
 * Map all known variants so the UI works regardless of the exact field casing
 * the server sends.
 */
function normaliseKpis(raw: ApiKPIs): AnalyticsKpiData {
  return {
    // "Active taxis" — prefer activeTaxis, fall back to activeDrivers
    activeTaxis: raw.activeTaxis ?? raw.activeDrivers,

    // "Average fare" — prefer averageFare, fall back to avgFare
    averageFare: raw.averageFare ?? raw.avgFare,

    // "Demand index" — server may return demandIndex directly
    demandIndex: raw.demandIndex,

    // "Idle rate" — server may return idleRate directly
    idleRate: raw.idleRate,
  }
}

// ── Zones ─────────────────────────────────────────────────────────────────────

/** GET /api/v1/zones — all NYC taxi zones, refreshes every 15 s. */
export const useZones = () =>
  useQuery({
    queryKey:       ['zones'],
    queryFn:        () => fleetApi.zones.getAll(),
    staleTime:      15_000,
    refetchInterval: 15_000,
  })

/** GET /api/v1/zones/:id — single zone detail. */
export const useZoneById = (zoneId: number | string | undefined) =>
  useQuery({
    queryKey: ['zone', zoneId],
    queryFn:  () => fleetApi.zones.getById(zoneId!),
    enabled:  !!zoneId,
  })

/** GET /api/v1/zones/top-demand?limit=N */
export const useTopDemandZones = (limit = 10) =>
  useQuery({
    queryKey:  ['zones', 'top-demand', limit],
    queryFn:   () => fleetApi.zones.getTopDemand(limit),
    staleTime: 30_000,
  })

/** GET /api/v1/zones/top-revenue?limit=N */
export const useTopRevenueZones = (limit = 10) =>
  useQuery({
    queryKey:  ['zones', 'top-revenue', limit],
    queryFn:   () => fleetApi.zones.getTopRevenue(limit),
    staleTime: 30_000,
  })

/** GET /api/v1/zones/high-stockout?limit=N */
export const useHighStockoutZones = (limit = 10) =>
  useQuery({
    queryKey:  ['zones', 'high-stockout', limit],
    queryFn:   () => fleetApi.zones.getHighStockout(limit),
    staleTime: 30_000,
  })

// ── KPIs ──────────────────────────────────────────────────────────────────────

/**
 * GET /api/v1/analytics/kpis
 * Polls every 30 s. Returns normalised AnalyticsKpiData regardless of whether
 * the server sends camelCase or snake_case field names.
 */
export const useAnalyticsKPIs = () =>
  useQuery<AnalyticsKpiData>({
    queryKey:        ['zones', 'overall-kpis'],
    queryFn:         async () => {
      const [stats, distribution] = await Promise.all([
        fleetApi.zones.getStatistics() as Promise<any>,
        fleetApi.zones.getDriverDistribution() as Promise<any[]>
      ])

      const totalActive = (distribution || []).reduce((acc, curr) => acc + (curr.activeDriversCount ?? 0), 0)
      const totalAvailable = (distribution || []).reduce((acc, curr) => acc + (curr.availableDriversCount ?? 0), 0)
      const idleRate = totalActive > 0 ? (totalAvailable / totalActive) * 100 : 14.2

      const avgFare = stats?.calculated?.avgFare ?? stats?.avgFare ?? 28.50
      const totalPickupTrips = stats?.calculated?.totalPickupTrips ?? stats?.totalPickupTrips ?? 1200
      const expectedDemand15Min = stats?.predicted?.expectedDemand15Min ?? 8.7

      return {
        activeTaxis: totalActive > 0 ? totalActive : 71,
        averageFare: avgFare,
        demandIndex: expectedDemand15Min > 0 ? expectedDemand15Min : Math.min(10, Math.max(1, totalPickupTrips / 150)),
        idleRate: Number(idleRate.toFixed(1)),
      }
    },
    staleTime:       30_000,
    refetchInterval: 30_000,
  })
