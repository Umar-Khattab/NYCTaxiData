'use client'

import * as React from 'react'
import {
  BarChart,
  Bar,
  CartesianGrid,
  XAxis,
  YAxis,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import type { SimulationResult } from '@/stores/use-simulation-engine-store'

interface ComparisonChartsProps {
  result: SimulationResult | null
  isLoading: boolean
}

export function ComparisonCharts({ result, isLoading }: ComparisonChartsProps) {
  const chartData = React.useMemo(() => {
    if (!result) return []
    return [
      {
        metric: 'Profit',
        baseline: result.baseline.profit,
        action: result.action.profit,
      },
      {
        metric: 'Revenue',
        baseline: result.baseline.revenue,
        action: result.action.revenue,
      },
      {
        metric: 'Cost',
        baseline: result.baseline.cost,
        action: result.action.cost,
      },
    ]
  }, [result])

  return (
    <div className="rounded-3xl border border-white/10 bg-slate-950/40 p-5 shadow-[0_0_40px_rgba(124,58,237,0.12)] backdrop-blur-2xl h-full flex flex-col">
      <div className="mb-4">
        <p className="text-sm text-muted-foreground">Scenario Comparison</p>
        <h2 className="text-xl font-semibold text-foreground">Profit Impact Analysis</h2>
      </div>

      {isLoading ? (
        <div className="flex-1 flex items-center justify-center">
          <div className="text-center">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
            <p className="mt-2 text-xs text-muted-foreground">Generating analysis...</p>
          </div>
        </div>
      ) : result ? (
        <div className="flex-1 space-y-4 overflow-y-auto">
          <div className="h-40">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={chartData} margin={{ top: 10, right: 6, left: -10, bottom: 0 }}>
                <CartesianGrid stroke="rgba(148,163,184,0.08)" vertical={false} />
                <XAxis dataKey="metric" axisLine={false} tickLine={false} tick={{ fill: '#94a3b8', fontSize: 11 }} />
                <YAxis axisLine={false} tickLine={false} tick={{ fill: '#94a3b8', fontSize: 11 }} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: 'rgba(15,23,42,0.92)',
                    border: '1px solid rgba(124,58,237,0.35)',
                    borderRadius: 12,
                    color: '#e2e8f0',
                  }}
                />
                <Legend wrapperStyle={{ color: '#94a3b8', fontSize: 12 }} />
                <Bar dataKey="baseline" fill="#6366f1" radius={[6, 6, 0, 0]} />
                <Bar dataKey="action" fill="#22c55e" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>

          <div className="grid gap-2">
            <div className="rounded-2xl border border-white/10 bg-white/5 p-3">
              <div className="flex items-center justify-between">
                <p className="text-xs text-muted-foreground">Profit Improvement</p>
                <Badge className="bg-emerald-500/20 text-emerald-300 border-emerald-500/30">
                  +${result.action.profit - result.baseline.profit}K
                </Badge>
              </div>
              <p className="mt-1 text-xs text-muted-foreground">
                {((((result.action.profit - result.baseline.profit) / result.baseline.profit) * 100).toFixed(1))}% improvement
              </p>
            </div>

            <div className="rounded-2xl border border-white/10 bg-white/5 p-3">
              <div className="flex items-center justify-between">
                <p className="text-xs text-muted-foreground">Demand Met</p>
                <Badge className="bg-cyan-500/20 text-cyan-300 border-cyan-500/30">
                  {result.action.demand_met}% vs {result.baseline.demand_met}%
                </Badge>
              </div>
              <p className="mt-1 text-xs text-muted-foreground">
                +{result.action.demand_met - result.baseline.demand_met}pp coverage
              </p>
            </div>
          </div>
        </div>
      ) : (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-xs text-muted-foreground text-center">Run simulation to see comparison results</p>
        </div>
      )}
    </div>
  )
}
