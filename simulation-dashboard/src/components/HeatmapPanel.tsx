import type { ZoneSimulationSnapshot } from '../types/simulation'

interface HeatmapPanelProps {
  zones: ZoneSimulationSnapshot[]
}

const MAX_COLUMNS = 6

export function HeatmapPanel({ zones }: HeatmapPanelProps) {
  const maxDemand = Math.max(1, ...zones.map((zone) => zone.demand))
  return (
    <div className="card heatmap-panel">
      <header>
        <h3>Zone Demand Heatmap</h3>
        <span>Demand intensity per zone</span>
      </header>
      <div
        className="heatmap-grid"
        style={{ gridTemplateColumns: `repeat(${MAX_COLUMNS}, 1fr)` }}
      >
        {zones.map((zone) => {
          const intensity = zone.demand / maxDemand
          const color = `rgba(255, 87, 51, ${0.2 + intensity * 0.8})`
          return (
            <div
              key={zone.zoneId}
              className="heatmap-cell"
              style={{ backgroundColor: color }}
            >
              <strong>Z{zone.zoneId}</strong>
              <span>{zone.demand.toFixed(1)}</span>
            </div>
          )
        })}
      </div>
    </div>
  )
}
