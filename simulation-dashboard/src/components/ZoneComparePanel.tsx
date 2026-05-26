import { useMemo, useState } from 'react'
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import type { ZoneMetricPoint } from '../types/simulation'

interface ZoneComparePanelProps {
  availableZones: number[]
  zoneHistory: Record<number, ZoneMetricPoint[]>
}

export function ZoneComparePanel({
  availableZones,
  zoneHistory,
}: ZoneComparePanelProps) {
  const [zoneA, setZoneA] = useState<number | null>(null)
  const [zoneB, setZoneB] = useState<number | null>(null)

  const zoneASelection = zoneA ?? availableZones[0] ?? null
  const zoneBSelection = zoneB ?? availableZones[1] ?? null

  const historyA = useMemo(
    () => (zoneASelection ? zoneHistory[zoneASelection] ?? [] : []),
    [zoneASelection, zoneHistory],
  )
  const historyB = useMemo(
    () => (zoneBSelection ? zoneHistory[zoneBSelection] ?? [] : []),
    [zoneBSelection, zoneHistory],
  )

  const chartData = useMemo(() => {
    const length = Math.max(historyA.length, historyB.length)
    return Array.from({ length }, (_, index) => ({
      time:
        historyA[index]?.simulatedTime ??
        historyB[index]?.simulatedTime ??
        '',
      zoneA: historyA[index]?.demand ?? null,
      zoneB: historyB[index]?.demand ?? null,
    }))
  }, [historyA, historyB])

  return (
    <div className="card compare-panel">
      <header>
        <h3>Zone Comparison</h3>
        <span>Compare demand between two zones</span>
      </header>
      <div className="compare-controls">
        <label>
          Zone A
          <select
            value={zoneASelection ?? ''}
            onChange={(event) => setZoneA(Number(event.target.value))}
          >
            {availableZones.map((zone) => (
              <option key={zone} value={zone}>
                Zone {zone}
              </option>
            ))}
          </select>
        </label>
        <label>
          Zone B
          <select
            value={zoneBSelection ?? ''}
            onChange={(event) => setZoneB(Number(event.target.value))}
          >
            {availableZones.map((zone) => (
              <option key={zone} value={zone}>
                Zone {zone}
              </option>
            ))}
          </select>
        </label>
      </div>
      <div className="chart-container">
        <ResponsiveContainer width="100%" height={220}>
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="time" hide />
            <YAxis />
            <Tooltip />
            <Legend />
            <Line
              type="monotone"
              dataKey="zoneA"
              name={zoneASelection ? `Zone ${zoneASelection}` : 'Zone A'}
              stroke="#f97316"
              strokeWidth={2}
              isAnimationActive
              animationDuration={400}
            />
            <Line
              type="monotone"
              dataKey="zoneB"
              name={zoneBSelection ? `Zone ${zoneBSelection}` : 'Zone B'}
              stroke="#0ea5e9"
              strokeWidth={2}
              isAnimationActive
              animationDuration={400}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
      <div className="compare-stats">
        <div>
          <strong>Zone A Revenue</strong>
          <span>{historyA.at(-1)?.revenue.toFixed(2) ?? '--'}</span>
        </div>
        <div>
          <strong>Zone B Revenue</strong>
          <span>{historyB.at(-1)?.revenue.toFixed(2) ?? '--'}</span>
        </div>
        <div>
          <strong>Zone A ETA</strong>
          <span>{historyA.at(-1)?.etaMinutes.toFixed(1) ?? '--'} min</span>
        </div>
        <div>
          <strong>Zone B ETA</strong>
          <span>{historyB.at(-1)?.etaMinutes.toFixed(1) ?? '--'} min</span>
        </div>
      </div>
    </div>
  )
}
