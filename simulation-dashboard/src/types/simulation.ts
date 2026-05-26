export interface SimulationStartRequest {
  durationHours: number
  speedFactor: number
  totalDrivers: number
  zoneCount: number
  startTime: string
}

export interface SimulationStatusResponse {
  simulationId: string
  status: string
  simulatedTime: string | null
  currentHour: number
  speedFactor: number
  isPaused: boolean
}

export interface SimulationAggregateMetrics {
  totalDemand: number
  totalRevenue: number
  avgEtaMinutes: number
  avgStockoutRisk: number
  totalDrivers: number
  totalActiveTrips: number
}

export interface ZoneSimulationSnapshot {
  zoneId: number
  driverCount: number
  activeTrips: number
  demand: number
  etaMinutes: number
  revenue: number
  stockoutRisk: number
}

export interface SimulationTick {
  simulationId: string
  simulatedTime: string
  hourIndex: number
  aggregate: SimulationAggregateMetrics
  zones: ZoneSimulationSnapshot[]
}

export interface ZoneMetricPoint {
  simulatedTime: string
  demand: number
  revenue: number
  etaMinutes: number
  stockoutRisk: number
  driverCount: number
  activeTrips: number
}
