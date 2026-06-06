'use client'

import * as React from 'react'
import { DeckGL } from '@deck.gl/react'
import { H3HexagonLayer } from '@deck.gl/geo-layers'
import { ArcLayer, ScatterplotLayer, TextLayer } from '@deck.gl/layers'
import { MapView, MapViewState } from '@deck.gl/core'
import Map from 'react-map-gl/maplibre'
import 'maplibre-gl/dist/maplibre-gl.css'
import { cn } from '@/lib/utils'
import * as h3 from 'h3-js'
import { formatZoneLabel } from '@/lib/nyc-zones'
import { InsightCard, type InsightCardData } from '@/components/insight-card'
import { fleetApi, type ApiZone } from '@/lib/fleet-api'

// ── Types ────────────────────────────────────────────────────────────────────
export interface FlowArc {
  from_zone: string
  to_zone: string
  from: [number, number]
  to: [number, number]
  count: number
  eta: string
}

export interface ZoneMetrics {
  hex: string
  demand: number
  supply: number
  forecast: number
  gap: number
  avgFare: number
  isHighProfit: boolean
}

interface TwinViewMapProps {
  className?: string
  flowArcs?: FlowArc[]
  zoneMetrics?: ZoneMetrics[]
  splitMode?: boolean
}

// ── Color helpers ────────────────────────────────────────────────────────────
const DEMAND_COLORS: [number, number, number][] = [
  [32, 24, 52],
  [124, 58, 237],
  [167, 139, 250],
  [248, 113, 113],
  [255, 50, 50],
]

function demandColor(value: number, max: number): [number, number, number] {
  const f = Math.min(Math.max(value / max, 0), 1) * (DEMAND_COLORS.length - 1)
  const i = Math.floor(f)
  const j = Math.min(i + 1, DEMAND_COLORS.length - 1)
  const t = f - i
  const c1 = DEMAND_COLORS[i], c2 = DEMAND_COLORS[j]
  return [
    Math.round(c1[0] + (c2[0] - c1[0]) * t),
    Math.round(c1[1] + (c2[1] - c1[1]) * t),
    Math.round(c1[2] + (c2[2] - c1[2]) * t),
  ]
}

function forecastColor(value: number, max: number): [number, number, number] {
  const f = Math.min(Math.max(value / max, 0), 1)
  return [
    Math.round(30 + f * 225),
    Math.round(20 + f * 160),
    Math.round(10 + f * 10),
  ]
}

function hexCenter(hex: string): [number, number] {
  try {
    const [lat, lng] = h3.cellToLatLng(hex)
    return [lng, lat]
  } catch {
    return [-74.006, 40.7128] // Fallback لنيويورك
  }
}

function buildDollarMarkers(zones: ZoneMetrics[]) {
  return zones.filter((z) => z.isHighProfit).map((z) => {
    const [lng, lat] = hexCenter(z.hex)
    return { position: [lng, lat] as [number, number], hex: z.hex, avgFare: z.avgFare }
  })
}

function usePulseAlpha(period = 1800): number {
  const [alpha, setAlpha] = React.useState(180)
  React.useEffect(() => {
    let id: number
    const tick = () => {
      const t = (Date.now() % period) / period
      setAlpha(Math.round(120 + 135 * Math.sin(Math.PI * 2 * t)))
      id = requestAnimationFrame(tick)
    }
    id = requestAnimationFrame(tick)
    return () => cancelAnimationFrame(id)
  }, [period])
  return alpha
}

const INITIAL_VIEW: MapViewState = {
  longitude: -74.006,
  latitude: 40.7128,
  zoom: 11.2,
  pitch: 45,
  bearing: -10,
}

const MAP_STYLE = 'https://basemaps.cartocdn.com/gl/dark-matter-gl-style/style.json'

function buildInsightData(zone: ZoneMetrics): InsightCardData {
  const max = 500
  return {
    zoneId: zone.hex,
    zoneName: formatZoneLabel(zone.hex),
    demandScore: Math.round((zone.demand / max) * 100),
    supplyScore: Math.round((zone.supply / max) * 100),
    trips: Math.floor(zone.demand),
    avgFare: zone.avgFare.toFixed(2),
    gap: Math.floor(zone.gap),
    profitTier: zone.isHighProfit ? 'high' : zone.avgFare > 30 ? 'medium' : 'low',
    hasDemandSpike: zone.demand > 350,
    explanation: zone.gap > 50
      ? `This zone is experiencing a demand surge of ${Math.floor(zone.gap)} units above current supply. Peak hours are driving trip requests faster than fleet coverage.`
      : zone.gap < -50
        ? `Fleet surplus of ${Math.abs(Math.floor(zone.gap))} vehicles with low demand signal. Consider repositioning excess vehicles to adjacent high-demand zones.`
        : `Supply and demand are relatively balanced in this sector. Avg fare of $${zone.avgFare.toFixed(2)} aligns with zone-level revenue targets.`,
    recommendation: zone.gap > 50
      ? `Dispatch ${Math.min(20, Math.floor(zone.gap / 5))} additional vehicles to zone immediately. High-profit potential—prioritize surge pricing.`
      : zone.isHighProfit
        ? `Maintain coverage. Dollar zone with above-average fare yield. Pre-position 5–8 vehicles for next demand peak.`
        : `Monitor for shift changes. No immediate action required unless demand index rises above 75%.`,
  }
}

// ── SingleMapPane ────────────────────────────────────────────────────────────
interface SingleMapPaneProps {
  label: string
  labelColor: string
  zones: ZoneMetrics[]
  flowArcs?: FlowArc[]
  showForecast?: boolean
  pulseAlpha: number
  onZoneClick?: (zone: ZoneMetrics, x: number, y: number) => void
  viewState: MapViewState
  onViewStateChange: (vs: MapViewState) => void
}

function SingleMapPane({
  label,
  labelColor,
  zones,
  flowArcs,
  showForecast = false,
  pulseAlpha,
  onZoneClick,
  viewState,
  onViewStateChange,
}: SingleMapPaneProps) {
  const spikeZones = zones.filter((z) => z.demand > 350)
  const dollarMarkers = buildDollarMarkers(zones)

  const hexLayer = new H3HexagonLayer<ZoneMetrics>({
    id: `h3-${label}`,
    data: zones,
    pickable: true,
    extruded: true,
    elevationScale: 8,
    coverage: 0.85,
    getHexagon: (d) => d.hex,
    getFillColor: (d) => {
      const base = showForecast ? forecastColor(d.forecast, 500) : demandColor(d.demand, 500)
      return [...base, 210] as [number, number, number, number]
    },
    getElevation: (d) => (showForecast ? d.forecast : d.demand),
    getLineColor: [255, 255, 255, 18],
    getLineWidth: 1,
    lineWidthMinPixels: 1,
    updateTriggers: { getFillColor: [showForecast, pulseAlpha], getElevation: [showForecast] },
    transitions: { getFillColor: 400, getElevation: 400 },
    onClick: (info) => {
      if (info.object && info.x != null && info.y != null) {
        onZoneClick?.(info.object as ZoneMetrics, info.x, info.y)
      }
    },
  })

  const pulseLayer = new ScatterplotLayer({
    id: `pulse-${label}`,
    data: spikeZones,
    pickable: false,
    getPosition: (d: ZoneMetrics) => hexCenter(d.hex),
    getFillColor: [248, 50, 50, 0],
    getLineColor: [248, 80, 80, pulseAlpha],
    stroked: true,
    filled: false,
    lineWidthMinPixels: 2,
    getRadius: 200,
    updateTriggers: { getLineColor: [pulseAlpha] },
  })

  const dollarTextLayer = new TextLayer({
    id: `dollar-text-${label}`,
    data: dollarMarkers,
    pickable: false,
    getPosition: (d) => [d.position[0], d.position[1], 180],
    getText: () => '$',
    getSize: 14,
    getColor: [253, 197, 0, 240],
    getAngle: 0,
    fontWeight: 'bold',
    sizeUnits: 'pixels',
    billboard: true,
  })

  const layers: any[] = [hexLayer, pulseLayer, dollarTextLayer]

  if (flowArcs && flowArcs.length > 0) {
    const arcLayer = new ArcLayer<FlowArc>({
      id: `arcs-${label}`,
      data: flowArcs,
      pickable: true,
      getSourcePosition: (d) => d.from,
      getTargetPosition: (d) => d.to,
      getSourceColor: [56, 189, 248, 200],
      getTargetColor: [34, 197, 94, 220],
      getWidth: (d) => Math.max(2, Math.min(8, d.count / 5)),
      widthUnits: 'pixels',
      greatCircle: false,
      getHeight: 0.5,
    })
    layers.push(arcLayer)

    const arcLabelLayer = new TextLayer<FlowArc>({
      id: `arc-labels-${label}`,
      data: flowArcs,
      pickable: false,
      getPosition: (d) => [
        (d.from[0] + d.to[0]) / 2,
        (d.from[1] + d.to[1]) / 2,
        120,
      ],
      getText: (d) => `${d.count} | ETA ${d.eta}`,
      getSize: 11,
      getColor: [255, 255, 255, 210],
      fontWeight: 'bold',
      sizeUnits: 'pixels',
      billboard: true,
    })
    layers.push(arcLabelLayer)
  }

  return (
    <div className="relative flex-1 h-full min-w-0 overflow-hidden">
      <DeckGL
        views={new MapView({ id: label, controller: true })}
        viewState={viewState}
        onViewStateChange={({ viewState: vs }) => onViewStateChange(vs as MapViewState)}
        layers={layers}
      >
        <Map mapStyle={MAP_STYLE} reuseMaps />
      </DeckGL>

      <div
        className="absolute top-3 left-3 z-20 px-3 py-1.5 rounded-full text-[10px] font-mono font-bold uppercase tracking-widest pointer-events-none"
        style={{
          background: 'rgba(5,4,15,0.82)',
          border: `1px solid ${labelColor}40`,
          color: labelColor,
          boxShadow: `0 0 12px ${labelColor}30`,
        }}
      >
        {label}
      </div>

      <div
        className="absolute bottom-3 left-3 z-20 p-2.5 rounded-xl text-[9px] pointer-events-none"
        style={{ background: 'rgba(5,4,15,0.82)', border: '1px solid rgba(255,255,255,0.07)' }}
      >
        <p className="font-mono font-bold uppercase tracking-widest text-slate-400 mb-1.5">
          {showForecast ? 'Forecast Demand' : 'Live Demand'}
        </p>
        <div className="flex items-center gap-1">
          {(showForecast
            ? [[30, 20, 10], [130, 100, 10], [255, 180, 10]] as [number, number, number][]
            : DEMAND_COLORS.slice(0, 4)
          ).map((c, i) => (
            <div
              key={i}
              className="size-3 rounded-sm"
              style={{ backgroundColor: `rgb(${c.join(',')})` }}
            />
          ))}
        </div>
        <div className="flex justify-between mt-1 text-slate-500">
          <span>Low</span>
          <span className={showForecast ? 'text-yellow-400' : 'text-red-400'}>Peak</span>
        </div>
      </div>
    </div>
  )
}

// ── Main TwinViewMap component ────────────────────────────────────────────────
export function TwinViewMap({ className, flowArcs = [], zoneMetrics, splitMode = true }: TwinViewMapProps) {
  const [zones, setZones] = React.useState<ZoneMetrics[]>([])
  const [loading, setLoading] = React.useState<boolean>(true)
  const [error, setError] = React.useState<string | null>(null)

  const pulseAlpha = usePulseAlpha(1800)
  const [leftView, setLeftView] = React.useState<MapViewState>(INITIAL_VIEW)
  const [rightView, setRightView] = React.useState<MapViewState>(INITIAL_VIEW)
  const [syncViews, setSyncViews] = React.useState(true)

  const [insightZone, setInsightZone] = React.useState<ZoneMetrics | null>(null)
  const [insightPos, setInsightPos] = React.useState({ x: 0, y: 0 })
  const containerRef = React.useRef<HTMLDivElement>(null)
  const [containerSize, setContainerSize] = React.useState({ width: 1000, height: 600 })

  // ── جلب البيانات الحقيقية من الـ API ─────────────────────────────────────────
  React.useEffect(() => {
    if (zoneMetrics) {
      setZones(zoneMetrics)
      setLoading(false)
      return
    }

    const loadRealData = async () => {
      try {
        setLoading(true)
        const rawZones: ApiZone[] = await fleetApi.zones.getAll()

        const mappedZones: ZoneMetrics[] = rawZones.map((z) => {
          const hex = h3.latLngToCell(z.latitude, z.longitude, 8)
          const demand = z.current_demand ?? 0
          const supply = z.current_drivers ?? 0
          return {
            hex,
            demand,
            supply,
            forecast: z.predicted_demand ?? 0,
            gap: demand - supply,
            avgFare: z.avg_fare ?? 0,
            isHighProfit: (z.avg_fare ?? 0) > 32,
          }
        })

        setZones(mappedZones)
      } catch (err: any) {
        console.error('Failed to load map data:', err)
        setError(err.message || 'Error connecting to fleet server')
      } finally {
        setLoading(false)
      }
    }

    loadRealData()
    
    const interval = setInterval(loadRealData, 10000)
    return () => clearInterval(interval)
  }, [zoneMetrics])

  React.useEffect(() => {
    if (!containerRef.current) return
    const ro = new ResizeObserver(() => {
      if (containerRef.current) {
        const { width, height } = containerRef.current.getBoundingClientRect()
        setContainerSize({ width, height })
      }
    })
    ro.observe(containerRef.current)
    return () => ro.disconnect()
  }, [])

  const handleLeftView = React.useCallback(
    (vs: MapViewState) => {
      setLeftView(vs)
      if (syncViews) setRightView(vs)
    },
    [syncViews]
  )

  const handleRightView = React.useCallback(
    (vs: MapViewState) => {
      setRightView(vs)
      if (syncViews) setLeftView(vs)
    },
    [syncViews]
  )

  const handleZoneClick = React.useCallback((zone: ZoneMetrics, x: number, y: number) => {
    setInsightZone(zone)
    setInsightPos({ x, y })
  }, [])

  if (loading && zones.length === 0) {
    return (
      <div className={cn('w-full h-full flex flex-col items-center justify-center bg-slate-950 text-white', className)}>
        <div className="size-10 border-4 border-t-purple-500 border-purple-900/30 rounded-full animate-spin mb-4"></div>
        <p className="font-mono text-xs tracking-widest text-purple-400 uppercase animate-pulse">Connecting to NYC Fleet Live Server...</p>
      </div>
    )
  }

  if (error && zones.length === 0) {
    return (
      <div className={cn('w-full h-full flex flex-col items-center justify-center bg-slate-950 text-red-400 p-4 border border-red-900/50 rounded-xl', className)}>
        <p className="font-bold text-sm mb-2">📡 Server Connection Failed</p>
        <p className="text-xs text-slate-500 font-mono">{error}</p>
      </div>
    )
  }

  return (
    <div ref={containerRef} className={cn('relative w-full h-full overflow-hidden', className)}>
      <div className="absolute inset-0 flex">
        <SingleMapPane
          label="BASELINE"
          labelColor="#6366f1"
          zones={zones}
          showForecast={false}
          pulseAlpha={pulseAlpha}
          onZoneClick={handleZoneClick}
          viewState={leftView}
          onViewStateChange={handleLeftView}
        />

        {splitMode && (
          <>
            <div
              className="w-[2px] z-30 relative flex-shrink-0"
              style={{
                background: 'linear-gradient(to bottom, transparent 0%, #7C3AED 30%, #38BDF8 70%, transparent 100%)',
                boxShadow: '0 0 16px rgba(124,58,237,0.6)',
              }}
            >
              <div
                className="absolute top-1/2 -translate-y-1/2 -translate-x-1/2 z-40 size-8 rounded-full flex items-center justify-center text-[9px] font-mono font-bold text-white select-none"
                style={{
                  background: 'linear-gradient(135deg, #7C3AED, #4F46E5)',
                  boxShadow: '0 0 20px rgba(124,58,237,0.7)',
                  border: '1px solid rgba(255,255,255,0.15)',
                }}
              >
                VS
              </div>
            </div>

            <SingleMapPane
              label="INTERVENTION"
              labelColor="#22c55e"
              zones={zones}
              flowArcs={flowArcs}
              showForecast={true}
              pulseAlpha={pulseAlpha}
              onZoneClick={handleZoneClick}
              viewState={rightView}
              onViewStateChange={handleRightView}
            />
          </>
        )}

        {!splitMode && (
          <SingleMapPane
            label="LIVE DEMAND"
            labelColor="#F87171"
            zones={zones}
            showForecast={false}
            pulseAlpha={pulseAlpha}
            onZoneClick={handleZoneClick}
            viewState={leftView}
            onViewStateChange={handleLeftView}
          />
        )}
      </div>

      {splitMode && (
        <button
          onClick={() => setSyncViews((s) => !s)}
          className="absolute top-3 left-1/2 -translate-x-1/2 z-40 px-3 py-1 rounded-full text-[9px] font-mono font-bold uppercase tracking-widest transition-all"
          style={{
            marginTop: 36,
            background: syncViews ? 'rgba(34,197,94,0.15)' : 'rgba(255,255,255,0.05)',
            border: `1px solid ${syncViews ? '#22c55e40' : 'rgba(255,255,255,0.1)'}`,
            color: syncViews ? '#4ade80' : '#9ca3af',
          }}
        >
          {syncViews ? '🔗 Views Synced' : '⛓ Sync Views'}
        </button>
      )}

      {insightZone && (
        <InsightCard
          data={buildInsightData(insightZone)}
          position={insightPos}
          containerSize={containerSize}
          onClose={() => setInsightZone(null)}
          onDispatch={() => setInsightZone(null)}
        />
      )}
    </div>
  )
}