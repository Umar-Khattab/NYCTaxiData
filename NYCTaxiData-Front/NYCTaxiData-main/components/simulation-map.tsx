'use client'

import * as React from 'react'
import DeckGL from '@deck.gl/react'
import { ScatterplotLayer, PathLayer } from '@deck.gl/layers'
import { HeatmapLayer } from '@deck.gl/aggregation-layers'
import { Badge } from '@/components/ui/badge'
import type { SimulationVehicle } from '@/stores/use-analytics-simulation-store'
import type { SimulationResult } from '@/stores/use-simulation-engine-store'

const INITIAL_VIEW_STATE = {
  longitude: -73.98,
  latitude: 40.76,
  zoom: 11.2,
  pitch: 40,
  bearing: -14,
}

interface SimulationMapProps {
  vehicles: SimulationVehicle[]
  simulationResult?: SimulationResult | null
  height?: string
}

export function SimulationMap({ vehicles, simulationResult, height = 'h-full' }: SimulationMapProps) {
  const mapRef = React.useRef<any>(null)
  const [mapSize, setMapSize] = React.useState({ width: 1000, height: 800 })
  const containerRef = React.useRef<HTMLDivElement>(null)

  // Handle container resize
  React.useEffect(() => {
    if (!containerRef.current) return

    const resizeObserver = new ResizeObserver(() => {
      if (containerRef.current) {
        const { width, height } = containerRef.current.getBoundingClientRect()
        setMapSize({ width: Math.max(width, 100), height: Math.max(height, 100) })
      }
    })

    resizeObserver.observe(containerRef.current)
    return () => resizeObserver.disconnect()
  }, [])

  // Scatterplot layer for vehicles
  const vehicleLayer = new ScatterplotLayer({
    id: 'vehicle-layer',
    data: vehicles,
    pickable: true,
    opacity: 0.85,
    stroked: true,
    filled: true,
    radiusScale: 12,
    radiusMinPixels: 5,
    radiusMaxPixels: 16,
    getPosition: (d: SimulationVehicle) => [d.lon, d.lat],
    getFillColor: (d: SimulationVehicle) =>
      d.status === 'idle'
        ? [56, 189, 248, 200] // Sky blue
        : d.status === 'charging'
          ? [249, 115, 22, 200] // Orange
          : [167, 139, 250, 200], // Purple
    getLineColor: [255, 255, 255, 255],
    getRadius: 80,
    onHover: (info: any) => {
      if (info.object) {
        // Could set tooltip here
      }
    },
  })

  // Heatmap layer for demand visualization
  const heatmapData = vehicles.map((v) => ({
    position: [v.lon, v.lat] as [number, number],
    weight: v.efficiency * 100,
  }))

  const heatmapLayer = new HeatmapLayer({
    id: 'heatmap-layer',
    data: heatmapData,
    getPosition: (d: any) => d.position,
    getWeight: (d: any) => d.weight,
    radiusPixels: 50,
    intensity: 0.5,
    threshold: 0.05,
    opacity: 0.4,
  })

  // Flow arcs layers for baseline and action paths
  const layers: any[] = [heatmapLayer, vehicleLayer]

  if (simulationResult?.flowArcs && simulationResult.flowArcs.length > 0) {
    // Baseline arcs
    const baselineArcs = simulationResult.flowArcs.filter((arc) => arc.type === 'baseline')
    if (baselineArcs.length > 0) {
      const baselineLayer = new PathLayer({
        id: 'baseline-arcs',
        data: baselineArcs,
        pickable: true,
        getPath: (d: any) => [d.from, d.to],
        getColor: [99, 102, 241, 180], // Indigo
        getWidth: 3,
        widthMinPixels: 2,
        widthMaxPixels: 8,
      })
      layers.push(baselineLayer)
    }

    // Action arcs
    const actionArcs = simulationResult.flowArcs.filter((arc) => arc.type === 'action')
    if (actionArcs.length > 0) {
      const actionLayer = new PathLayer({
        id: 'action-arcs',
        data: actionArcs,
        pickable: true,
        getPath: (d: any) => [d.from, d.to],
        getColor: [34, 197, 94, 220], // Green
        getWidth: 4,
        widthMinPixels: 3,
        widthMaxPixels: 10,
      })
      layers.push(actionLayer)
    }
  }

  return (
    <div
      ref={containerRef}
      className={`${height} rounded-3xl border border-white/10 bg-slate-950/30 shadow-[0_0_40px_rgba(124,58,237,0.18)] backdrop-blur-2xl overflow-hidden flex flex-col`}
    >
      <div className="flex items-center justify-between px-5 py-4 border-b border-white/10 bg-slate-950/60">
        <div>
          <p className="text-sm font-medium text-muted-foreground">Interactive Mobility Simulation</p>
          <h2 className="text-xl font-semibold text-foreground">3D Fleet & Flow Visualization</h2>
        </div>
        <div className="flex gap-2">
          <Badge className="bg-emerald-500/20 text-emerald-200 border-emerald-500/30">Action</Badge>
          <Badge className="bg-indigo-500/20 text-indigo-200 border-indigo-500/30">Baseline</Badge>
        </div>
      </div>

      <div className="relative flex-1 overflow-hidden">
        <DeckGL
          ref={mapRef}
          initialViewState={INITIAL_VIEW_STATE}
          controller={true}
          layers={layers}
          width={mapSize.width}
          height={mapSize.height}
          style={{ width: '100%', height: '100%' }}
        />
        <div className="pointer-events-none absolute left-4 bottom-4 rounded-2xl border border-white/10 bg-slate-950/75 px-3 py-2 text-[12px] text-slate-200 backdrop-blur-md">
          {simulationResult
            ? 'Flow arcs show repositioning paths | Green=Action, Indigo=Baseline'
            : 'Vehicle distribution and heatmap visualization'}
        </div>
      </div>
    </div>
  )
}
