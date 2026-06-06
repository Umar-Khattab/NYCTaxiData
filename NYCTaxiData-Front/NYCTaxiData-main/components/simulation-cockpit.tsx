'use client'

import * as React from 'react'
import { Sparkles, Zap, AlertCircle } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Loader2 } from 'lucide-react'

interface SimulationCockpitProps {
  isLoading: boolean
  onReset: () => void
  status: 'ready' | 'running' | 'completed' | 'error'
  lastRunTime?: string
}

export function SimulationCockpit({ isLoading, onReset, status, lastRunTime }: SimulationCockpitProps) {
  const statusConfig = {
    ready: {
      icon: Sparkles,
      label: 'Ready',
      color: 'bg-emerald-500/20 text-emerald-200 border-emerald-500/30',
      pulse: false,
    },
    running: {
      icon: Zap,
      label: 'Running Simulation',
      color: 'bg-yellow-500/20 text-yellow-200 border-yellow-500/30 animate-pulse',
      pulse: true,
    },
    completed: {
      icon: Sparkles,
      label: 'Analysis Complete',
      color: 'bg-emerald-500/20 text-emerald-200 border-emerald-500/30',
      pulse: false,
    },
    error: {
      icon: AlertCircle,
      label: 'Error',
      color: 'bg-red-500/20 text-red-200 border-red-500/30',
      pulse: false,
    },
  }

  const config = statusConfig[status]
  const Icon = config.icon

  return (
    <div className="rounded-3xl border border-white/10 bg-slate-950/40 p-5 shadow-[0_0_40px_rgba(124,58,237,0.12)] backdrop-blur-2xl">
      <div className="space-y-4">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="inline-flex items-center gap-2 rounded-full border border-violet-500/15 bg-violet-500/10 px-3 py-1">
              <Sparkles className="size-4 text-violet-300" />
              <span className="text-xs font-semibold text-violet-200">Simulation Cockpit</span>
            </div>
          </div>
          <Button
            variant="outline"
            size="sm"
            onClick={onReset}
            disabled={isLoading}
            className="text-xs h-8"
          >
            Reset Layout
          </Button>
        </div>

        {/* Status Display */}
        <div className="grid gap-3 md:grid-cols-3">
          <div className="rounded-2xl border border-white/10 bg-white/5 p-3">
            <div className="flex items-center gap-2">
              <Icon className={`size-5 ${config.pulse ? 'animate-pulse' : ''}`} style={{ color: 'currentColor' }} />
              <div>
                <p className="text-xs text-muted-foreground">Status</p>
                <p className="text-sm font-semibold text-foreground">{config.label}</p>
              </div>
            </div>
          </div>

          <div className="rounded-2xl border border-white/10 bg-white/5 p-3">
            <p className="text-xs text-muted-foreground">Simulation Engine</p>
            <Badge className="mt-1 bg-primary/20 text-primary border-primary/30">v2.1 Ready</Badge>
          </div>

          <div className="rounded-2xl border border-white/10 bg-white/5 p-3">
            <p className="text-xs text-muted-foreground">Last Run</p>
            <p className="text-sm font-semibold text-foreground">{lastRunTime || 'Never'}</p>
          </div>
        </div>

        {/* System Health */}
        <div className="rounded-2xl border border-white/10 bg-slate-950/50 p-3">
          <p className="mb-2 text-xs font-semibold uppercase tracking-[0.1em] text-muted-foreground">System Health</p>
          <div className="space-y-1 text-xs">
            <div className="flex items-center justify-between">
              <span className="text-muted-foreground">Data Pipeline</span>
              <span className="inline-flex items-center gap-1 rounded-full bg-emerald-500/20 px-2 py-0.5 text-emerald-300">
                <span className="size-1.5 rounded-full bg-emerald-400" />
                Nominal
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-muted-foreground">API Connectivity</span>
              <span className="inline-flex items-center gap-1 rounded-full bg-emerald-500/20 px-2 py-0.5 text-emerald-300">
                <span className="size-1.5 rounded-full bg-emerald-400" />
                Connected
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-muted-foreground">Memory Usage</span>
              <span className="inline-flex items-center gap-1 rounded-full bg-yellow-500/20 px-2 py-0.5 text-yellow-300">
                <span className="size-1.5 rounded-full bg-yellow-400" />
                72%
              </span>
            </div>
          </div>
        </div>

        {/* Quick Info */}
        <div className="rounded-2xl border border-white/10 bg-white/5 p-3">
          <p className="text-xs text-muted-foreground">
            Configure simulation parameters below and click <span className="font-semibold text-foreground">Run Simulation</span> to analyze predicted outcomes with baseline and action scenarios.
          </p>
        </div>
      </div>
    </div>
  )
}
