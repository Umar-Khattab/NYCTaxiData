import { create } from 'zustand'
import type { MapZoneSelection } from '@/lib/dispatch-types'

interface DispatchState {
  selectedZoneId: string | null
  selectedZoneLabel: string | null
  selectedZoneMetrics: MapZoneSelection | null
  /** Bumped on each successful zone dispatch (for queue / toast side effects). */
  lastDispatchAt: number | null
  setSelectedZone: (zone: MapZoneSelection) => void
  clearSelectedZone: () => void
  /** Run dispatch for a zone — updates selection + signals listeners. */
  dispatchZone: (zone: MapZoneSelection) => void
}

export const useDispatchStore = create<DispatchState>((set) => ({
  selectedZoneId: null,
  selectedZoneLabel: null,
  selectedZoneMetrics: null,
  lastDispatchAt: null,
  setSelectedZone: (zone) =>
    set({
      selectedZoneId: zone.zoneId,
      selectedZoneLabel: zone.zone,
      selectedZoneMetrics: zone,
    }),
  clearSelectedZone: () =>
    set({
      selectedZoneId: null,
      selectedZoneLabel: null,
      selectedZoneMetrics: null,
    }),
  dispatchZone: (zone) =>
    set({
      selectedZoneId: zone.zoneId,
      selectedZoneLabel: zone.zone,
      selectedZoneMetrics: zone,
      lastDispatchAt: Date.now(),
    }),
}))
