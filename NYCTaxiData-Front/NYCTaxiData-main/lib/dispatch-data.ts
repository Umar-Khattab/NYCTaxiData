/**
 * dispatch-data.ts
 *
 * Live dispatch data utilities.
 * Replaces the old dispatch-mock-data.ts random generators with API-bound
 * equivalents and stable zone metadata.
 */

import { fleetApi, type ApiDispatch } from '@/lib/fleet-api'
import type { DispatchRequest, AutoDispatchRule } from '@/lib/dispatch-types'
import { getNYCDispatchHexPool, formatZoneLabel } from '@/lib/nyc-zones'

// ── Stable zone pool (H3 resolution-8, no randomness) ────────────────────────

export const DISPATCH_ZONE_HEXES = getNYCDispatchHexPool(8)

/** Human-readable NYC zone names keyed by index (used when the API omits a name). */
const FALLBACK_ZONE_NAMES = [
  'Times Square',
  'JFK Airport',
  'Brooklyn Bridge',
  'Central Park',
  'Wall Street',
  'Midtown',
  'Williamsburg',
  'Harlem',
]

export const DISPATCH_ZONE_OPTIONS = DISPATCH_ZONE_HEXES.map((zoneId, i) => ({
  zoneId,
  label: FALLBACK_ZONE_NAMES[i] ?? formatZoneLabel(zoneId),
}))

// ── API → DispatchRequest adapter ─────────────────────────────────────────────

function toDispatchRequest(d: ApiDispatch): DispatchRequest {
  const elapsedMs = Date.now() - new Date(d.createdAt).getTime()
  const elapsedMin = Math.max(1, Math.floor(elapsedMs / 60_000))

  return {
    id: String(d.tripId),
    zoneId: String(d.pickupZoneId),
    zone: d.pickupZoneName ?? `Zone ${d.pickupZoneId}`,
    priority:
      d.priority === 'CRITICAL' ? 'high'
      : d.priority === 'HIGH'     ? 'medium'
      :                             'low',
    driversNeeded: 1,
    driversAssigned: d.driverId ? 1 : 0,
    status: d.status?.toLowerCase().includes('complet') ? 'completed'
          : d.status?.toLowerCase().includes('progress') ? 'in-progress'
          : d.status?.toLowerCase().includes('fail')     ? 'failed'
          : 'pending',
    timestamp: `${elapsedMin}m ago`,
    eta: '—',
  }
}

/**
 * Fetches the live dispatch queue from the API.
 * Falls back to an empty array on error (caller decides how to surface it).
 */
export async function generateDispatchRequests(): Promise<DispatchRequest[]> {
  try {
    const raw = await fleetApi.dispatches.getAll()
    return raw.map(toDispatchRequest)
  } catch (err) {
    console.error('[dispatch-data] Failed to load dispatch queue:', err)
    return []
  }
}

// ── Auto-dispatch rules (static config — no randomness) ──────────────────────

export const defaultAutoDispatchRules: AutoDispatchRule[] = [
  {
    id: '1',
    name: 'High Demand Surge',
    condition: 'Demand > 150% baseline',
    action: 'Deploy 20% reserve fleet',
    enabled: true,
  },
  {
    id: '2',
    name: 'Airport Queue',
    condition: 'JFK queue > 50 passengers',
    action: 'Route 15 nearest vehicles',
    enabled: true,
  },
  {
    id: '3',
    name: 'Low Coverage Alert',
    condition: 'Zone coverage < 60%',
    action: 'Redistribute idle drivers',
    enabled: false,
  },
  {
    id: '4',
    name: 'Rush Hour Prep',
    condition: '30 min before peak',
    action: 'Pre-position vehicles',
    enabled: true,
  },
]
