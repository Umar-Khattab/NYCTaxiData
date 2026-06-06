'use client'

import * as React from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Slider } from '@/components/ui/slider'
import { Badge } from '@/components/ui/badge'
import { Loader2, Play } from 'lucide-react'
import type { SimulationInput } from '@/stores/use-simulation-engine-store'

interface SimulationInputPanelProps {
  input: SimulationInput
  onInputChange: (updates: Partial<SimulationInput>) => void
  onSimulate: () => Promise<void>
  isLoading: boolean
}

export function SimulationInputPanel({
  input,
  onInputChange,
  onSimulate,
  isLoading,
}: SimulationInputPanelProps) {
  return (
    <div className="rounded-3xl border border-white/10 bg-slate-950/40 p-5 shadow-[0_0_40px_rgba(124,58,237,0.12)] backdrop-blur-2xl h-full flex flex-col">
      <div className="mb-5">
        <p className="text-sm text-muted-foreground">Simulation Input</p>
        <h2 className="text-xl font-semibold text-foreground">Configuration Panel</h2>
      </div>

      <div className="flex-1 space-y-5 overflow-y-auto">
        {/* Target DateTime */}
        <div className="space-y-2">
          <Label className="text-xs font-semibold text-muted-foreground uppercase tracking-[0.1em]">
            Target Simulation Time
          </Label>
          <Input
            type="datetime-local"
            value={input.target_datetime}
            onChange={(e) => onInputChange({ target_datetime: e.target.value })}
            className="bg-white/5 border-white/10 text-foreground placeholder:text-muted-foreground"
          />
          <p className="text-[11px] text-muted-foreground">
            Forecast horizon for simulation analysis
          </p>
        </div>

        {/* Action Type */}
        <div className="space-y-2">
          <Label className="text-xs font-semibold text-muted-foreground uppercase tracking-[0.1em]">
            Action Type
          </Label>
          <Select value={input.action_type} onValueChange={(v: any) => onInputChange({ action_type: v })}>
            <SelectTrigger className="bg-white/5 border-white/10 text-foreground">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="reposition">Fleet Repositioning</SelectItem>
              <SelectItem value="expansion">Service Expansion</SelectItem>
              <SelectItem value="none">Baseline Only</SelectItem>
            </SelectContent>
          </Select>
          <p className="text-[11px] text-muted-foreground">
            Strategy to evaluate against baseline scenario
          </p>
        </div>

        {/* Constraints Section */}
        <div className="space-y-3 rounded-2xl border border-white/10 bg-white/5 p-3">
          <p className="text-xs font-semibold text-muted-foreground uppercase tracking-[0.1em]">
            Constraints
          </p>

          {/* Max Vehicles */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <Label className="text-xs text-muted-foreground">Max Vehicles</Label>
              <Badge className="bg-primary/20 text-primary border-primary/30">
                {input.constraints.max_vehicles}
              </Badge>
            </div>
            <Slider
              value={[input.constraints.max_vehicles]}
              min={10}
              max={200}
              step={10}
              onValueChange={(value) =>
                onInputChange({
                  constraints: {
                    ...input.constraints,
                    max_vehicles: value[0],
                  },
                })
              }
              aria-label="Max vehicles"
            />
            <p className="text-[10px] text-muted-foreground">Available fleet size for action</p>
          </div>

          {/* Budget Limit */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <Label className="text-xs text-muted-foreground">Budget Limit ($K)</Label>
              <Badge className="bg-emerald-500/20 text-emerald-300 border-emerald-500/30">
                ${input.constraints.budget_limit}K
              </Badge>
            </div>
            <Slider
              value={[input.constraints.budget_limit]}
              min={1000}
              max={50000}
              step={1000}
              onValueChange={(value) =>
                onInputChange({
                  constraints: {
                    ...input.constraints,
                    budget_limit: value[0],
                  },
                })
              }
              aria-label="Budget limit"
            />
            <p className="text-[10px] text-muted-foreground">Capital allocation ceiling</p>
          </div>

          {/* Service Level */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <Label className="text-xs text-muted-foreground">Service Level SLA (%)</Label>
              <Badge className="bg-amber-500/20 text-amber-300 border-amber-500/30">
                {input.constraints.service_level}%
              </Badge>
            </div>
            <Slider
              value={[input.constraints.service_level]}
              min={70}
              max={99}
              step={1}
              onValueChange={(value) =>
                onInputChange({
                  constraints: {
                    ...input.constraints,
                    service_level: value[0],
                  },
                })
              }
              aria-label="Service level"
            />
            <p className="text-[10px] text-muted-foreground">Target demand fulfillment rate</p>
          </div>
        </div>
      </div>

      {/* Simulate Button */}
      <div className="mt-5 space-y-3">
        <Button
          onClick={onSimulate}
          disabled={isLoading}
          className="w-full bg-gradient-to-r from-emerald-600 to-cyan-600 hover:from-emerald-700 hover:to-cyan-700 text-white font-semibold gap-2 h-10"
        >
          {isLoading ? (
            <>
              <Loader2 className="size-4 animate-spin" />
              Running...
            </>
          ) : (
            <>
              <Play className="size-4" />
              Run Simulation
            </>
          )}
        </Button>
        <p className="text-[11px] text-muted-foreground text-center">
          {isLoading ? 'Processing simulation...' : 'Click to generate baseline and action scenarios'}
        </p>
      </div>
    </div>
  )
}
