import { create } from 'zustand'
import { fleetApi, type ApiVehicle, type ApiPrediction } from '@/lib/fleet-api'

// ── Public types ──────────────────────────────────────────────────────────────

export interface SimulationVehicle {
  id: string
  lon: number
  lat: number
  status: 'idle' | 'active' | 'charging'
  efficiency: number
}

export interface PredictionSnapshot {
  time: string
  demand: number
  supply: number
  gap: number
  revenue: number
  confidence: number
  utilization: number
  trendLabel: string
  zoneForecast: Array<{
    zone: string
    predicted: number
    confidence: number
  }>
}

// ── API → local type adapters ─────────────────────────────────────────────────

function toSimulationVehicle(v: ApiVehicle): SimulationVehicle {
  return {
    id: v.vehicle_id,
    lon: v.longitude,
    lat: v.latitude,
    // 'en-route' and 'offline' map to 'active' / 'idle' respectively
    status:
      v.status === 'charging'
        ? 'charging'
        : v.status === 'idle' || v.status === 'offline'
          ? 'idle'
          : 'active',
    efficiency: v.efficiency ?? 0.8,
  }
}

function toPredictionSnapshot(p: ApiPrediction): PredictionSnapshot {
  return {
    time: p.time,
    demand: p.demand,
    supply: p.supply,
    gap: p.gap,
    revenue: p.revenue,
    confidence: p.confidence,
    utilization: p.utilization,
    trendLabel: p.trend === 'rising' ? '↑ Rising' : p.trend === 'declining' ? '↓ Declining' : '→ Stable',
    zoneForecast: (p.zone_forecasts ?? []).map((zf) => ({
      zone: zf.zone,
      predicted: zf.predicted,
      confidence: zf.confidence,
    })),
  }
}

// ── Store ─────────────────────────────────────────────────────────────────────

interface SimulationState {
  selectedStep: number
  timeLabels: string[]
  vehicles: SimulationVehicle[]
  snapshots: PredictionSnapshot[]
  isLoading: boolean
  error: string | null

  setSelectedStep: (step: number) => void
  fetchData: () => Promise<void>

  /** @deprecated — kept for backward-compat; prefer vehicles/snapshots directly */
  simulationData: {
    vehicles: SimulationVehicle[]
    snapshots: PredictionSnapshot[]
  }
}

export const useAnalyticsSimulationStore = create<SimulationState>((set, get) => ({
  selectedStep: 0,
  timeLabels: [],
  vehicles: [],
  snapshots: [],
  isLoading: false,
  error: null,

  simulationData: { vehicles: [], snapshots: [] },

  setSelectedStep: (step) => set({ selectedStep: step }),

  fetchData: async () => {
    set({ isLoading: true, error: null })
    try {
      const [rawVehicles, rawSnapshots] = await Promise.all([
        fleetApi.vehicles.getAll(),
        fleetApi.predictions.getSnapshots(48),
      ])

      const vehicles = rawVehicles.map(toSimulationVehicle)
      const snapshots = rawSnapshots.map(toPredictionSnapshot)
      const timeLabels = snapshots.map((s) => s.time)

      set({
        vehicles,
        snapshots,
        timeLabels,
        simulationData: { vehicles, snapshots },
        isLoading: false,
      })
    } catch (err) {
      set({
        error: err instanceof Error ? err.message : 'Failed to load analytics data',
        isLoading: false,
      })
    }
  },
}))

// ── Selector helpers ──────────────────────────────────────────────────────────

/** Returns the vehicles array for a given step index (step is ignored — all are live). */
export function getSimulationVehicles(_step: number): SimulationVehicle[] {
  return useAnalyticsSimulationStore.getState().vehicles
}

export function getPredictionSnapshot(step: number): PredictionSnapshot | null {
  const { snapshots } = useAnalyticsSimulationStore.getState()
  return snapshots[step] ?? snapshots[0] ?? null
}
