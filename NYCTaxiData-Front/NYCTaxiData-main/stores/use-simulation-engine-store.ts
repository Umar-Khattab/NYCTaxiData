import { create } from 'zustand'
import { fleetApi, type ApiSimulationInput, type ApiSimulationResult } from '@/lib/fleet-api'
// Re-export shapes the rest of the app already imports from here
export interface SimulationInput {
  target_datetime: string
  action_type: 'reposition' | 'expansion' | 'none'
  constraints: {
    max_vehicles: number
    budget_limit: number
    service_level: number
  }
}

export interface SimulationResult {
  baseline: {
    profit: number
    demand_met: number
    fleet_utilization: number
    revenue: number
    cost: number
  }
  action: {
    profit: number
    demand_met: number
    fleet_utilization: number
    revenue: number
    cost: number
  }
  p50_impact: number
  p90_impact: number
  recommendation: string
  flowArcs: Array<{
    from: [number, number]
    to: [number, number]
    type: 'baseline' | 'action'
    volume: number
  }>
}

interface SimulationEngineState {
  input: SimulationInput
  result: SimulationResult | null
  isLoading: boolean
  error: string | null
  setInput: (input: Partial<SimulationInput>) => void
  runSimulation: (input: SimulationInput) => Promise<void>
  clearResult: () => void
  setError: (error: string | null) => void
}

const defaultInput: SimulationInput = {
  target_datetime: new Date().toISOString().slice(0, 16),
  action_type: 'reposition',
  constraints: {
    max_vehicles: 50,
    budget_limit: 10000,
    service_level: 95,
  },
}

export const useSimulationEngineStore = create<SimulationEngineState>((set) => ({
  input: defaultInput,
  result: null,
  isLoading: false,
  error: null,

  setInput: (updates) =>
    set((state) => ({
      input: {
        ...state.input,
        ...updates,
        constraints: {
          ...state.input.constraints,
          ...(updates.constraints ?? {}),
        },
      },
    })),

  runSimulation: async (input) => {
    set({ isLoading: true, error: null })
    try {
      const result = await fleetApi.simulation.run(input)
      set({ result, isLoading: false })
    } catch (err) {
      set({
        error: err instanceof Error ? err.message : 'Simulation failed',
        isLoading: false,
      })
    }
  },

  clearResult: () => set({ result: null }),
  setError: (error) => set({ error }),
}))
