'use client'

import * as React from 'react'
import {
  Play, Loader2, RefreshCw, ChevronRight, ChevronDown, Plus, Trash2,
  BrainCircuit, Target, DollarSign, TrendingUp, TrendingDown,
  Clock, Zap, BarChart3, ArrowRight, Settings2,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Slider } from '@/components/ui/slider'
import { Badge } from '@/components/ui/badge'
import { ScrollArea } from '@/components/ui/scroll-area'
import { cn } from '@/lib/utils'
import type { FlowArc } from '@/components/twin-view-map'
import { fleetApi } from '@/lib/fleet-api'
import {
  BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid,
} from 'recharts'

// ── Types ─────────────────────────────────────────────────────────────────────

export interface RepositionAction {
  id: string
  fromZone: string
  toZone: string
  vehicleCount: number
}

export interface CounterfactualInput {
  targetTime: string
  actions: RepositionAction[]
  maxVehicles: number
  budgetLimit: number
}

export interface ScenarioKPIs {
  revenue: number
  avgWaitTime: number
  demandMet: number
  fleetUtil: number
  profit: number
}

export interface CounterfactualResult {
  baseline: ScenarioKPIs
  intervention: ScenarioKPIs
  flowArcs: FlowArc[]
  recommendation: string
}

interface CounterfactualPanelProps {
  className?: string
  onResult?: (result: CounterfactualResult) => void
  onFlowArcsChange?: (arcs: FlowArc[]) => void
}

// ── NYC zone reference data (stable geographic coords) ───────────────────────

const NYC_ZONES = [
  { id: 'midtown',    label: 'Midtown Manhattan',  lat: 40.756, lng: -73.987 },
  { id: 'downtown',  label: 'Downtown / FiDi',     lat: 40.709, lng: -74.009 },
  { id: 'jfk',       label: 'JFK Airport',          lat: 40.641, lng: -73.778 },
  { id: 'times_sq',  label: 'Times Square',         lat: 40.758, lng: -73.985 },
  { id: 'brooklyn',  label: 'Brooklyn Bridge',      lat: 40.706, lng: -73.997 },
  { id: 'harlem',    label: 'Harlem',               lat: 40.811, lng: -73.946 },
  { id: 'lga',       label: 'LaGuardia Airport',    lat: 40.776, lng: -73.874 },
  { id: 'upper_east',label: 'Upper East Side',      lat: 40.773, lng: -73.958 },
]

function getZoneCoords(id: string): [number, number] {
  const z = NYC_ZONES.find((z) => z.id === id) ?? NYC_ZONES[0]
  return [z.lng, z.lat]
}

// ── KPI delta card ────────────────────────────────────────────────────────────

function KPIDeltaCard({
  label,
  baseline,
  intervention,
  unit,
  lowerIsBetter = false,
  icon: Icon,
}: {
  label: string
  baseline: number
  intervention: number
  unit: string
  lowerIsBetter?: boolean
  icon: React.ElementType
}) {
  const delta = intervention - baseline
  const improved = lowerIsBetter ? delta < 0 : delta > 0
  const pct = baseline !== 0 ? ((Math.abs(delta) / Math.abs(baseline)) * 100).toFixed(1) : '0'

  return (
    <div
      className="rounded-xl p-3 flex flex-col gap-1.5"
      style={{
        background: 'rgba(255,255,255,0.03)',
        border: '1px solid rgba(255,255,255,0.07)',
      }}
    >
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-1.5">
          <Icon className="size-3 text-slate-400" />
          <span className="text-[9px] font-mono font-bold uppercase tracking-widest text-slate-400">{label}</span>
        </div>
        <span
          className="text-[9px] font-bold px-1.5 py-0.5 rounded-full"
          style={{
            background: improved ? 'rgba(34,197,94,0.15)' : 'rgba(248,113,113,0.15)',
            color: improved ? '#4ade80' : '#f87171',
            border: `1px solid ${improved ? 'rgba(34,197,94,0.3)' : 'rgba(248,113,113,0.3)'}`,
          }}
        >
          {improved ? '▲' : '▼'} {pct}%
        </span>
      </div>
      <div className="grid grid-cols-2 gap-2">
        <div>
          <p className="text-[8px] text-slate-500 mb-0.5">Baseline</p>
          <p className="text-sm font-black font-mono text-slate-300">
            {typeof baseline === 'number' && baseline > 1000
              ? `$${(baseline / 1000).toFixed(1)}K`
              : `${baseline.toFixed(1)}${unit}`}
          </p>
        </div>
        <div>
          <p className="text-[8px] text-slate-500 mb-0.5">Intervention</p>
          <p className="text-sm font-black font-mono" style={{ color: improved ? '#4ade80' : '#f87171' }}>
            {typeof intervention === 'number' && intervention > 1000
              ? `$${(intervention / 1000).toFixed(1)}K`
              : `${intervention.toFixed(1)}${unit}`}
          </p>
        </div>
      </div>
    </div>
  )
}

// ── Comparison bar chart ──────────────────────────────────────────────────────

function ComparisonBarChart({ result }: { result: CounterfactualResult }) {
  const data = [
    { name: 'Revenue', baseline: result.baseline.revenue / 1000, intervention: result.intervention.revenue / 1000 },
    { name: 'Profit', baseline: result.baseline.profit / 1000, intervention: result.intervention.profit / 1000 },
    { name: 'Demand%', baseline: result.baseline.demandMet, intervention: result.intervention.demandMet },
    { name: 'Fleet%', baseline: result.baseline.fleetUtil, intervention: result.intervention.fleetUtil },
  ]

  return (
    <div className="h-36">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={data} margin={{ top: 4, right: 4, left: -20, bottom: 0 }}>
          <CartesianGrid stroke="rgba(255,255,255,0.04)" vertical={false} />
          <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{ fill: '#64748b', fontSize: 9 }} />
          <YAxis axisLine={false} tickLine={false} tick={{ fill: '#64748b', fontSize: 9 }} />
          <Tooltip
            contentStyle={{
              background: 'rgba(10,8,20,0.95)',
              border: '1px solid rgba(124,58,237,0.3)',
              borderRadius: 8,
              color: '#e2e8f0',
              fontSize: 11,
            }}
          />
          <Bar dataKey="baseline" name="Baseline" fill="#6366f1" radius={[4, 4, 0, 0]} opacity={0.8} />
          <Bar dataKey="intervention" name="Intervention" fill="#22c55e" radius={[4, 4, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}

// ── Action row ────────────────────────────────────────────────────────────────

function ActionRow({
  action,
  index,
  onChange,
  onRemove,
}: {
  action: RepositionAction
  index: number
  onChange: (a: RepositionAction) => void
  onRemove: () => void
}) {
  return (
    <div
      className="p-3 rounded-xl space-y-2"
      style={{
        background: 'rgba(124,58,237,0.06)',
        border: '1px solid rgba(124,58,237,0.18)',
      }}
    >
      <div className="flex items-center justify-between">
        <span className="text-[10px] font-mono font-bold text-violet-400 uppercase tracking-widest">
          Reposition #{index + 1}
        </span>
        <button
          onClick={onRemove}
          className="text-slate-500 hover:text-red-400 transition-colors"
        >
          <Trash2 className="size-3.5" />
        </button>
      </div>

      <div className="grid grid-cols-2 gap-2">
        <div className="space-y-1">
          <Label className="text-[9px] text-slate-500 uppercase tracking-wider">From Zone</Label>
          <select
            value={action.fromZone}
            onChange={(e) => onChange({ ...action, fromZone: e.target.value })}
            className="w-full h-8 rounded-lg text-[11px] font-mono text-white px-2 outline-none"
            style={{
              background: 'rgba(255,255,255,0.05)',
              border: '1px solid rgba(255,255,255,0.1)',
            }}
          >
            {NYC_ZONES.map((z) => (
              <option key={z.id} value={z.id} style={{ background: '#0f0e1a' }}>
                {z.label}
              </option>
            ))}
          </select>
        </div>
        <div className="space-y-1">
          <Label className="text-[9px] text-slate-500 uppercase tracking-wider">To Zone</Label>
          <select
            value={action.toZone}
            onChange={(e) => onChange({ ...action, toZone: e.target.value })}
            className="w-full h-8 rounded-lg text-[11px] font-mono text-white px-2 outline-none"
            style={{
              background: 'rgba(255,255,255,0.05)',
              border: '1px solid rgba(255,255,255,0.1)',
            }}
          >
            {NYC_ZONES.filter((z) => z.id !== action.fromZone).map((z) => (
              <option key={z.id} value={z.id} style={{ background: '#0f0e1a' }}>
                {z.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      <div className="space-y-1">
        <div className="flex items-center justify-between">
          <Label className="text-[9px] text-slate-500 uppercase tracking-wider">Vehicles</Label>
          <Badge
            className="text-[9px] h-4 px-1.5"
            style={{ background: 'rgba(139,92,246,0.2)', color: '#a78bfa', border: '1px solid rgba(139,92,246,0.3)' }}
          >
            {action.vehicleCount}
          </Badge>
        </div>
        <Slider
          value={[action.vehicleCount]}
          min={1}
          max={40}
          step={1}
          onValueChange={(v) => onChange({ ...action, vehicleCount: v[0] })}
          className="py-1"
        />
      </div>
    </div>
  )
}

// ── Main CounterfactualPanel ──────────────────────────────────────────────────

export function CounterfactualPanel({ className, onResult, onFlowArcsChange }: CounterfactualPanelProps) {
  const [input, setInput] = React.useState<CounterfactualInput>({
    targetTime: new Date(Date.now() + 3600_000).toISOString().slice(0, 16),
    actions: [
      { id: '1', fromZone: 'harlem', toZone: 'midtown', vehicleCount: 12 },
    ],
    maxVehicles: 50,
    budgetLimit: 10000,
  })
  const [isLoading, setIsLoading] = React.useState(false)
  const [result, setResult] = React.useState<CounterfactualResult | null>(null)
  const [showConfig, setShowConfig] = React.useState(true)
  const [status, setStatus] = React.useState<'idle' | 'running' | 'done'>('idle')

  const addAction = () => {
    const newId = String(Date.now())
    setInput((p) => ({
      ...p,
      actions: [...p.actions, { id: newId, fromZone: 'brooklyn', toZone: 'times_sq', vehicleCount: 8 }],
    }))
  }

  const updateAction = (id: string, updated: RepositionAction) => {
    setInput((p) => ({ ...p, actions: p.actions.map((a) => (a.id === id ? updated : a)) }))
  }

  const removeAction = (id: string) => {
    setInput((p) => ({ ...p, actions: p.actions.filter((a) => a.id !== id) }))
  }

  const runSimulation = async () => {
    setIsLoading(true)
    setStatus('running')
    try {
      const apiResult = await fleetApi.counterfactual.evaluate({
        target_datetime: input.targetTime,
        max_vehicles: input.maxVehicles,
        budget_limit: input.budgetLimit,
        actions: input.actions.map((a) => ({
          from_zone: a.fromZone,
          to_zone: a.toZone,
          vehicle_count: a.vehicleCount,
          from_coords: getZoneCoords(a.fromZone),
          to_coords: getZoneCoords(a.toZone),
        })),
      })

      const res: CounterfactualResult = {
        baseline: {
          revenue: apiResult.baseline.revenue,
          avgWaitTime: apiResult.baseline.avg_wait_time,
          demandMet: apiResult.baseline.demand_met,
          fleetUtil: apiResult.baseline.fleet_utilization,
          profit: apiResult.baseline.profit,
        },
        intervention: {
          revenue: apiResult.intervention.revenue,
          avgWaitTime: apiResult.intervention.avg_wait_time,
          demandMet: apiResult.intervention.demand_met,
          fleetUtil: apiResult.intervention.fleet_utilization,
          profit: apiResult.intervention.profit,
        },
        flowArcs: apiResult.flow_arcs.map((arc: any) => ({
          from_zone: arc.from_zone,
          to_zone: arc.to_zone,
          from: arc.from,
          to: arc.to,
          count: arc.count,
          eta: arc.eta,
        })),
        recommendation: apiResult.recommendation,
      }

      setResult(res)
      setStatus('done')
      onResult?.(res)
      onFlowArcsChange?.(res.flowArcs)
    } catch (err) {
      console.error('[counterfactual] simulation failed:', err)
      setStatus('idle')
    } finally {
      setIsLoading(false)
    }
  }

  const reset = () => {
    setResult(null)
    setStatus('idle')
    onFlowArcsChange?.([])
  }

  return (
    <div
      className={cn('flex flex-col h-full overflow-hidden', className)}
      style={{
        background: 'linear-gradient(180deg, rgba(8,6,20,0.97) 0%, rgba(6,5,16,0.97) 100%)',
        borderLeft: '1px solid rgba(139,92,246,0.15)',
      }}
    >
      {/* Header */}
      <div
        className="flex-shrink-0 px-4 py-4 border-b"
        style={{ borderColor: 'rgba(139,92,246,0.15)' }}
      >
        <div className="flex items-center gap-2.5 mb-1">
          <div
            className="size-7 rounded-lg flex items-center justify-center"
            style={{
              background: 'linear-gradient(135deg, rgba(124,58,237,0.3), rgba(79,70,229,0.2))',
              border: '1px solid rgba(124,58,237,0.4)',
              boxShadow: '0 0 12px rgba(124,58,237,0.3)',
            }}
          >
            <BrainCircuit className="size-3.5 text-violet-300" />
          </div>
          <div>
            <h2 className="text-sm font-bold text-white leading-none">Counterfactual</h2>
            <p className="text-[10px] text-slate-500 mt-0.5">Simulation Engine</p>
          </div>
          <div className="ml-auto flex items-center gap-1.5">
            <span
              className={cn(
                'size-1.5 rounded-full',
                status === 'running' ? 'bg-yellow-400 animate-pulse' : status === 'done' ? 'bg-emerald-400' : 'bg-violet-500'
              )}
            />
            <span
              className="text-[9px] font-mono font-bold uppercase tracking-wider"
              style={{ color: status === 'running' ? '#facc15' : status === 'done' ? '#4ade80' : '#a78bfa' }}
            >
              {status === 'running' ? 'Analyzing' : status === 'done' ? 'Complete' : 'Ready'}
            </span>
          </div>
        </div>
      </div>

      <ScrollArea className="flex-1 min-h-0">
        <div className="p-4 space-y-4">

          {/* Target time */}
          <div className="space-y-1.5">
            <Label className="text-[9px] font-mono font-bold uppercase tracking-widest text-slate-400">
              Target Simulation Time
            </Label>
            <Input
              type="datetime-local"
              value={input.targetTime}
              onChange={(e) => setInput((p) => ({ ...p, targetTime: e.target.value }))}
              className="h-9 text-xs font-mono"
              style={{
                background: 'rgba(255,255,255,0.04)',
                border: '1px solid rgba(255,255,255,0.1)',
                color: '#e2e8f0',
              }}
            />
          </div>

          {/* Config collapsible */}
          <div>
            <button
              onClick={() => setShowConfig((s) => !s)}
              className="w-full flex items-center justify-between py-1.5 text-[9px] font-mono font-bold uppercase tracking-widest text-slate-500 hover:text-slate-300 transition-colors"
            >
              <div className="flex items-center gap-1.5">
                <Settings2 className="size-3" />
                Constraints
              </div>
              {showConfig ? <ChevronDown className="size-3" /> : <ChevronRight className="size-3" />}
            </button>
            {showConfig && (
              <div className="space-y-3 pt-1">
                <div className="space-y-1.5">
                  <div className="flex items-center justify-between">
                    <Label className="text-[9px] text-slate-500 uppercase tracking-wider">Max Vehicles</Label>
                    <Badge style={{ background: 'rgba(139,92,246,0.2)', color: '#a78bfa', border: '1px solid rgba(139,92,246,0.3)' }} className="text-[9px] h-4 px-1.5">{input.maxVehicles}</Badge>
                  </div>
                  <Slider value={[input.maxVehicles]} min={5} max={200} step={5}
                    onValueChange={(v) => setInput((p) => ({ ...p, maxVehicles: v[0] }))} />
                </div>
                <div className="space-y-1.5">
                  <div className="flex items-center justify-between">
                    <Label className="text-[9px] text-slate-500 uppercase tracking-wider">Budget Limit</Label>
                    <Badge style={{ background: 'rgba(34,197,94,0.15)', color: '#4ade80', border: '1px solid rgba(34,197,94,0.25)' }} className="text-[9px] h-4 px-1.5">${(input.budgetLimit / 1000).toFixed(0)}K</Badge>
                  </div>
                  <Slider value={[input.budgetLimit]} min={1000} max={50000} step={1000}
                    onValueChange={(v) => setInput((p) => ({ ...p, budgetLimit: v[0] }))} />
                </div>
              </div>
            )}
          </div>

          {/* Actions */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <span className="text-[9px] font-mono font-bold uppercase tracking-widest text-slate-400">
                Reposition Actions ({input.actions.length})
              </span>
              <button
                onClick={addAction}
                className="flex items-center gap-1 text-[9px] font-bold text-violet-400 hover:text-violet-300 transition-colors"
              >
                <Plus className="size-3" /> Add
              </button>
            </div>
            <div className="space-y-2">
              {input.actions.map((a, i) => (
                <ActionRow
                  key={a.id}
                  action={a}
                  index={i}
                  onChange={(updated) => updateAction(a.id, updated)}
                  onRemove={() => removeAction(a.id)}
                />
              ))}
            </div>
          </div>

          {/* Run button */}
          <Button
            onClick={runSimulation}
            disabled={isLoading || input.actions.length === 0}
            className="w-full h-10 text-sm font-bold gap-2"
            style={{
              background: 'linear-gradient(135deg, #7C3AED 0%, #4F46E5 50%, #0ea5e9 100%)',
              boxShadow: isLoading ? 'none' : '0 0 20px rgba(124,58,237,0.5)',
            }}
          >
            {isLoading ? (
              <><Loader2 className="size-4 animate-spin" /> Simulating...</>
            ) : (
              <><Play className="size-4" /> Run Counterfactual</>
            )}
          </Button>

          {/* Results */}
          {result && !isLoading && (
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <span className="text-[9px] font-mono font-bold uppercase tracking-widest text-emerald-400">
                  ✓ Simulation Results
                </span>
                <button onClick={reset} className="text-[9px] text-slate-500 hover:text-slate-300 flex items-center gap-1">
                  <RefreshCw className="size-3" /> Reset
                </button>
              </div>

              {/* Recommendation */}
              <div className="p-3 rounded-xl text-[11px] leading-relaxed"
                style={{ background: 'rgba(34,197,94,0.07)', border: '1px solid rgba(34,197,94,0.2)' }}>
                <p className="text-emerald-300">{result.recommendation}</p>
              </div>

              {/* Comparison bar chart */}
              <div>
                <p className="text-[9px] font-mono font-bold uppercase tracking-widest text-slate-500 mb-2">
                  Scenario Comparison
                </p>
                <ComparisonBarChart result={result} />
              </div>

              {/* KPI delta cards */}
              <div className="grid grid-cols-1 gap-2">
                <KPIDeltaCard label="Revenue" baseline={result.baseline.revenue} intervention={result.intervention.revenue} unit="" icon={DollarSign} />
                <KPIDeltaCard label="Avg Wait Time" baseline={result.baseline.avgWaitTime} intervention={result.intervention.avgWaitTime} unit=" min" lowerIsBetter icon={Clock} />
                <KPIDeltaCard label="Demand Met" baseline={result.baseline.demandMet} intervention={result.intervention.demandMet} unit="%" icon={Target} />
                <KPIDeltaCard label="Fleet Util" baseline={result.baseline.fleetUtil} intervention={result.intervention.fleetUtil} unit="%" icon={Zap} />
              </div>

              {/* Flow arcs summary */}
              {result.flowArcs.length > 0 && (
                <div>
                  <p className="text-[9px] font-mono font-bold uppercase tracking-widest text-slate-500 mb-2">
                    Repositioning Plan
                  </p>
                  <div className="space-y-1.5">
                    {result.flowArcs.map((arc, i) => (
                      <div key={i} className="flex items-center gap-2 p-2 rounded-lg text-[10px]"
                        style={{ background: 'rgba(255,255,255,0.03)', border: '1px solid rgba(255,255,255,0.06)' }}>
                        <span className="text-slate-400 truncate flex-1">{arc.from_zone.replace('_', ' ')}</span>
                        <ArrowRight className="size-3 text-violet-400 flex-shrink-0" />
                        <span className="text-white truncate flex-1">{arc.to_zone.replace('_', ' ')}</span>
                        <Badge className="text-[9px] h-4 px-1.5 flex-shrink-0"
                          style={{ background: 'rgba(34,197,94,0.15)', color: '#4ade80', border: '1px solid rgba(34,197,94,0.25)' }}>
                          {arc.count}v · {arc.eta}
                        </Badge>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      </ScrollArea>
    </div>
  )
}
