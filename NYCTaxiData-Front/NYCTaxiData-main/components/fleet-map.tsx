'use client'

import * as React from 'react'
import { DeckGL } from '@deck.gl/react'
import { MapView, PickingInfo, MapViewState } from '@deck.gl/core'
import { ScatterplotLayer } from '@deck.gl/layers'
import { createVehicleScenegraphLayer } from '@/components/vehicle-scenegraph-layer'
import type { FleetVehicleData } from '@/lib/fleet-vehicle'
import Map from 'react-map-gl/maplibre'
import 'maplibre-gl/dist/maplibre-gl.css'
import { cn } from '@/lib/utils'
import { useTheme } from 'next-themes'

interface FleetVehicle {
  id: string
  longitude: number
  latitude: number
  heading: number
  status: string
  driver: string
}

function toFleetVehicleData(vehicles: FleetVehicle[]): FleetVehicleData[] {
  return vehicles.map((v) => ({
    id: v.id,
    latitude: v.latitude,
    longitude: v.longitude,
    bearing: v.heading,
  }))
}

interface FleetMapProps {
  vehicles: FleetVehicle[]
  onSelectVehicle: (vehicle: FleetVehicle) => void
  selectedVehicleId?: string
  className?: string
}

export function FleetMap({ vehicles, onSelectVehicle, selectedVehicleId, className }: FleetMapProps) {
  const { theme } = useTheme()
  const [viewState, setViewState] = React.useState<MapViewState>({
    longitude: -74.0060,
    latitude: 40.7128,
    zoom: 14,
    pitch: 45,
    bearing: 0
  })

  const getStatusColorRGB = (status: string): [number, number, number] => {
    switch (status) {
      case 'active': return [34, 197, 94] // Green
      case 'en-route': return [139, 92, 246] // Purple
      case 'idle': return [245, 158, 11] // Orange
      default: return [100, 116, 139] // Gray
    }
  }

  const layers = [
    new ScatterplotLayer({
      id: 'shadow-layer',
      data: vehicles,
      getPosition: (d: FleetVehicle) => [d.longitude, d.latitude],
      getFillColor: [0, 0, 0, 80],
      getRadius: 15,
      radiusMinPixels: 10,
      radiusMaxPixels: 25,
      transitions: {
        getPosition: 2000
      }
    }),
    new ScatterplotLayer({
      id: 'glow-layer',
      data: vehicles,
      getPosition: (d: FleetVehicle) => [d.longitude, d.latitude],
      getFillColor: (d: FleetVehicle) => [...getStatusColorRGB(d.status), d.status === 'offline' ? 0 : 150],
      getRadius: 25,
      radiusMinPixels: 15,
      radiusMaxPixels: 35,
      transitions: {
        getPosition: 2000
      }
    }),
    createVehicleScenegraphLayer({
      id: 'fleet-layer',
      data: toFleetVehicleData(vehicles),
      transitionMs: 2000,
      onClick: ({ object }: PickingInfo<FleetVehicleData>) => {
        if (object) {
          const vehicle = vehicles.find((v) => v.id === object.id)
          if (vehicle) onSelectVehicle(vehicle)
        }
      },
    }),
  ]

  const mapStyle = theme === 'dark' 
    ? "https://basemaps.cartocdn.com/gl/dark-matter-gl-style/style.json"
    : "https://basemaps.cartocdn.com/gl/voyager-gl-style/style.json"

  return (
    <div className={cn("relative w-full h-full overflow-hidden rounded-xl border border-border bg-background", className)}>
      <DeckGL
        views={new MapView({ id: 'map', controller: true })}
        initialViewState={viewState}
        onViewStateChange={({ viewState }) => setViewState(viewState)}
        layers={layers}
        getTooltip={({ object }: PickingInfo<FleetVehicle>) => 
          object ? {
            html: `
              <div class="p-3 bg-white/90 dark:bg-black/80 backdrop-blur-md border border-border rounded-lg shadow-xl">
                <div class="text-[10px] font-bold text-primary uppercase tracking-widest mb-1">${object.id}</div>
                <div class="text-sm font-bold text-foreground">${object.driver}</div>
                <div class="text-[9px] text-muted-foreground mt-1 uppercase">${object.status}</div>
              </div>
            `,
            style: { backgroundColor: 'transparent', padding: '0' }
          } : null
        }
      >
        <Map
          mapStyle={mapStyle}
          reuseMaps
        />
      </DeckGL>

      {/* Map Overlays */}
      <div className="absolute top-4 left-4 z-10 pointer-events-none">
        <div className="px-3 py-1.5 bg-background/80 backdrop-blur-md border border-border rounded-full flex items-center gap-2 shadow-lg">
          <div className="size-2 bg-success rounded-full animate-pulse" />
          <span className="text-[10px] font-bold text-foreground tracking-widest uppercase">Precision Fleet Tracking</span>
        </div>
      </div>
    </div>
  )
}
