import type { SimulationTick } from '../types/simulation'

interface StatsPanelProps {
  tick: SimulationTick | null
}

export function StatsPanel({ tick }: StatsPanelProps) {
  const aggregate = tick?.aggregate
  return (
    <div className="card stats-panel">
      <header>
        <h3>Snapshot Metrics</h3>
        <span>Live system totals</span>
      </header>
      <div className="stats-grid">
        <div>
          <strong>Total Demand</strong>
          <span>{aggregate?.totalDemand.toFixed(1) ?? '--'}</span>
        </div>
        <div>
          <strong>Total Revenue</strong>
          <span>${aggregate?.totalRevenue.toFixed(2) ?? '--'}</span>
        </div>
        <div>
          <strong>Avg ETA</strong>
          <span>{aggregate?.avgEtaMinutes.toFixed(1) ?? '--'} min</span>
        </div>
        <div>
          <strong>Stockout Risk</strong>
          <span>{aggregate?.avgStockoutRisk.toFixed(2) ?? '--'}</span>
        </div>
        <div>
          <strong>Active Trips</strong>
          <span>{aggregate?.totalActiveTrips ?? '--'}</span>
        </div>
        <div>
          <strong>Total Drivers</strong>
          <span>{aggregate?.totalDrivers ?? '--'}</span>
        </div>
      </div>
    </div>
  )
}
