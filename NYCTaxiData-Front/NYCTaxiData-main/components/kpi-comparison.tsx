'use client'

import * as React from 'react'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { TrendingUp, TrendingDown, Activity, Zap } from 'lucide-react'
import type { SimulationResult } from '@/stores/use-simulation-engine-store'

interface KPIComparisonProps {
  result: SimulationResult | null
  isLoading: boolean
}

interface KPIItem {
  label: string
  icon: React.ReactNode
  baseline: string | number
  action: string | number
  unit: string
  improvement: number
  percentage: boolean
}

export function KPIComparison({ result, isLoading }: KPIComparisonProps) {
  const kpis = React.useMemo((): KPIItem[] => {
    if (!result) return []
    return [
      {
        label: 'Unmet Demand',
        icon: <Activity className="size-4" />,
        baseline: `${100 - result.baseline.demand_met}%`,
        action: `${100 - result.action.demand_met}%`,
        unit: 'orders',
        improvement: result.baseline.demand_met - result.action.demand_met,
        percentage: false,
      },
      {
        label: 'Fleet Utilization',
        icon: <Zap className="size-4" />,
        baseline: `${result.baseline.fleet_utilization}%`,
        action: `${result.action.fleet_utilization}%`,
        unit: '%',
        improvement: result.action.fleet_utilization - result.baseline.fleet_utilization,
        percentage: false,
      },
    ]
  }, [result])

  return (
    <div className="rounded-3xl border border-white/10 bg-slate-950/40 p-5 shadow-[0_0_40px_rgba(124,58,237,0.12)] backdrop-blur-2xl h-full flex flex-col">
      <div className="mb-4">
        <p className="text-sm text-muted-foreground">Performance Metrics</p>
        <h2 className="text-xl font-semibold text-foreground">KPI Changes</h2>
      </div>

      {isLoading ? (
        <div className="flex-1 flex items-center justify-center">
          <div className="text-center">
            <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-primary" />
            <p className="mt-2 text-xs text-muted-foreground">Computing metrics...</p>
          </div>
        </div>
      ) : result ? (
        <div className="flex-1 space-y-3 overflow-y-auto">
          {kpis.map((kpi) => {
            const improvementColor = kpi.improvement >= 0 
              ? 'text-emerald-300' 
              : 'text-red-300'
            const improvementBg = kpi.improvement >= 0 
              ? 'bg-emerald-500/20 border-emerald-500/30' 
              : 'bg-red-500/20 border-red-500/30'
            const TrendIcon = kpi.improvement >= 0 ? TrendingUp : TrendingDown

            return (
              <Card
                key={kpi.label}
                className="rounded-2xl bg-white/5 border border-white/10 p-3"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="flex gap-3 flex-1">
                    <div className="inline-flex h-8 w-8 items-center justify-center rounded-lg border border-white/10 bg-white/5 text-muted-foreground">
                      {kpi.icon}
                    </div>
                    <div className="flex-1">
                      <p className="text-xs font-semibold text-muted-foreground uppercase tracking-[0.1em]">
                        {kpi.label}
                      </p>
                      <div className="mt-1 grid grid-cols-2 gap-2 text-xs">
                        <div>
                          <span className="text-muted-foreground">Baseline: </span>
                          <span className="font-semibold text-foreground">{kpi.baseline}</span>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Action: </span>
                          <span className="font-semibold text-foreground">{kpi.action}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                  <Badge className={`${improvementBg} ${improvementColor} border whitespace-nowrap`}>
                    <TrendIcon className="size-3 mr-1" />
                    {kpi.improvement >= 0 ? '+' : ''}{kpi.improvement.toFixed(1)}{kpi.unit}
                  </Badge>
                </div>
              </Card>
            )
          })}
        </div>
      ) : (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-xs text-muted-foreground text-center">Run simulation to see KPI changes</p>
        </div>
      )}
    </div>
  )
}
