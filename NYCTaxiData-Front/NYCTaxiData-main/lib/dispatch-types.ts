export interface MapZoneSelection {
  zoneId: string
  zone: string
  /** Active passenger / dispatch requests in zone */
  trips: number
  avgFare: string
  /** Available drivers in zone */
  supply: number
  gap: number
}

/** Hex click payload from Deck.gl (screen position + zone metrics). */
export interface ZoneMapClickEvent {
  zone: MapZoneSelection
  /** Pixel offset relative to the map container */
  position: { x: number; y: number }
}

export type DispatchPriority = 'high' | 'medium' | 'low'
export type DispatchStatus = 'pending' | 'in-progress' | 'completed' | 'failed'

export interface DispatchRequest {
  id: string
  zoneId: string
  zone: string
  priority: DispatchPriority
  driversNeeded: number
  driversAssigned: number
  status: DispatchStatus
  timestamp: string
  eta: string
}

export interface AutoDispatchRule {
  id: string
  name: string
  condition: string
  action: string
  enabled: boolean
}
