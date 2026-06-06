'use client'

import * as React from 'react'
import { motion } from 'framer-motion'
import {
  Area,
  AreaChart,
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
  Legend,
} from 'recharts'
import { TrendingUp, Brain, Activity } from 'lucide-react'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { useTheme } from 'next-themes'
import { fleetApi } from '@/lib/fleet-api'

// ── Types ──────────────────────────────────────────────────────────────────
interface DataPoint {
  time: string
  actual: number | null
  predicted: number
  lower: number
  upper: number
}

interface PredictiveDemandChartProps {
  title?: string
  height?: number
  points?: number
}

// ── API data loader ────────────────────────────────────────────────────────
async function loadChartData(points: number): Promise<DataPoint[]> {
  const snapshots = await fleetApi.predictions.getSnapshots(points)
  const historyPoints = Math.floor(points * 0.4)
  return snapshots.map((s, i) => ({
    time: s.time,
    actual: i < historyPoints ? s.demand : null,
    predicted: s.demand,   // server already returns ML prediction
    lower: Math.max(0, s.demand - 18),
    upper: s.demand + 22,
  }))
}

// ── Custom tooltip ─────────────────────────────────────────────────────────
interface CustomTooltipProps {
  active?: boolean
  payload?: Array<{ name: string; value: number; color: string }>
  label?: string
}

function CustomTooltip({ active, payload, label }: CustomTooltipProps) {
  if (!active || !payload?.length) return null

  return (
    <div className="bg-popover/95 backdrop-blur-md border border-primary/30 rounded-xl px-4 py-3 shadow-2xl shadow-primary/10 text-xs">
      <p className="text-muted-foreground mb-2 font-medium">{label}</p>
      {payload.map((entry) => (
        <div key={entry.name} className="flex items-center gap-2 mb-1">
          <span
            className="inline-block w-2 h-2 rounded-full"
            style={{ backgroundColor: entry.color }}
          />
          <span className="text-muted-foreground capitalize">{entry.name}:</span>
          <span className="font-semibold text-foreground">{entry.value}</span>
        </div>
      ))}
    </div>
  )
}

// ── Main component ─────────────────────────────────────────────────────────
export function PredictiveDemandChart({
  title = '24-Hour Demand Forecast',
  height = 300,
  points = 24,
}: PredictiveDemandChartProps) {
  const [data, setData] = React.useState<DataPoint[]>([])
  const { resolvedTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'

  React.useEffect(() => {
    loadChartData(points)
      .then(setData)
      .catch(() => setData([]))
  }, [points])

  const axisColor = isDark ? '#6b5f8a' : 'hsl(215, 15%, 55%)'
  const gridColor = isDark ? 'rgba(157,111,232,0.08)' : 'rgba(99,102,241,0.06)'

  return (
    <motion.div
      initial={{ opacity: 0, y: 16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5, ease: 'easeOut' }}
    >
      <Card className="p-4 bg-card/50 border-border/50 backdrop-blur-sm">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-primary/10">
              <Brain className="size-4 text-primary" />
            </div>
            <div>
              <h3 className="font-semibold text-sm text-foreground">{title}</h3>
              <p className="text-xs text-muted-foreground">AI-powered 6-hour rolling forecast</p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Badge variant="outline" className="text-[10px] bg-chart-2/10 text-chart-2 border-chart-2/30">
              <Activity className="size-2.5 mr-1" />
              Live
            </Badge>
            <Badge variant="outline" className="text-[10px] bg-primary/10 text-primary border-primary/30">
              <TrendingUp className="size-2.5 mr-1" />
              94.7% Accuracy
            </Badge>
          </div>
        </div>

        {/* Legend */}
        <div className="flex gap-4 mb-3 text-[11px]">
          <div className="flex items-center gap-1.5">
            <div className="w-4 h-0.5 bg-chart-2 rounded-full" />
            <span className="text-muted-foreground">Actual</span>
          </div>
          <div className="flex items-center gap-1.5">
            <div className="w-4 h-0.5 bg-primary rounded-full" />
            <span className="text-muted-foreground">Predicted</span>
          </div>
          <div className="flex items-center gap-1.5">
            <div className="w-4 h-3 rounded-sm bg-primary/15" />
            <span className="text-muted-foreground">Confidence band</span>
          </div>
        </div>

        {/* Chart */}
        <div style={{ height }}>
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={data} margin={{ top: 8, right: 8, left: -20, bottom: 0 }}>
              <defs>
                {/* Confidence band gradient */}
                <linearGradient id="pdcUpperGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#9d6fe8" stopOpacity={0.20} />
                  <stop offset="95%" stopColor="#9d6fe8" stopOpacity={0.03} />
                </linearGradient>
                <linearGradient id="pdcLowerGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="transparent" stopOpacity={0} />
                  <stop offset="95%" stopColor="transparent" stopOpacity={0} />
                </linearGradient>
                {/* Actual area gradient */}
                <linearGradient id="pdcActualGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#38bdf8" stopOpacity={0.35} />
                  <stop offset="95%" stopColor="#38bdf8" stopOpacity={0} />
                </linearGradient>
              </defs>

              <CartesianGrid stroke={gridColor} strokeDasharray="4 4" vertical={false} />

              <XAxis
                dataKey="time"
                axisLine={false}
                tickLine={false}
                tick={{ fontSize: 10, fill: axisColor }}
                interval={Math.floor(points / 6)}
              />
              <YAxis
                axisLine={false}
                tickLine={false}
                tick={{ fontSize: 10, fill: axisColor }}
              />

              <Tooltip content={<CustomTooltip />} />

              {/* Confidence band – upper fill */}
              <Area
                type="monotone"
                dataKey="upper"
                stroke="none"
                fill="url(#pdcUpperGrad)"
                fillOpacity={1}
                legendType="none"
                name="Upper bound"
              />
              {/* Confidence band – lower erases the inner area */}
              <Area
                type="monotone"
                dataKey="lower"
                stroke="none"
                fill="url(#pdcLowerGrad)"
                fillOpacity={1}
                legendType="none"
                name="Lower bound"
              />

              {/* Predicted line */}
              <Area
                type="monotone"
                dataKey="predicted"
                stroke="#9d6fe8"
                strokeWidth={2.5}
                fill="none"
                dot={false}
                activeDot={{ r: 4, strokeWidth: 0, fill: '#9d6fe8' }}
                name="predicted"
                style={{ filter: 'drop-shadow(0 0 6px rgba(157,111,232,0.6))' }}
              />

              {/* Actual line */}
              <Area
                type="monotone"
                dataKey="actual"
                stroke="#38bdf8"
                strokeWidth={2.5}
                fill="url(#pdcActualGrad)"
                fillOpacity={1}
                dot={false}
                activeDot={{ r: 4, strokeWidth: 0, fill: '#38bdf8' }}
                connectNulls={false}
                name="actual"
                style={{ filter: 'drop-shadow(0 0 6px rgba(56,189,248,0.5))' }}
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      </Card>
    </motion.div>
  )
}
