'use client'

import * as React from 'react'
import { cn } from '@/lib/utils'
import {
  ContextMenu,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuTrigger,
} from '@/components/ui/context-menu'
import { Button } from '@/components/ui/button'
import { Switch } from '@/components/ui/switch'
import { Slider } from '@/components/ui/slider'
import { Send, BarChart3, Bell, Layers, ChevronDown, ChevronUp, Eye, EyeOff } from 'lucide-react'

interface HexCell {
  id: string
  x: number
  y: number
  trips: number
  avgFare: number
  zone: string
}

// Generate hexagonal grid data for NYC
function generateHexGrid(): HexCell[] {
  const hexes: HexCell[] = []
  const zones = [
    'Manhattan', 'Brooklyn', 'Queens', 'Bronx', 'Staten Island',
    'JFK Airport', 'LaGuardia', 'Times Square', 'Wall Street', 'Central Park'
  ]
  
  const rows = 12
  const cols = 16
  
  for (let row = 0; row < rows; row++) {
    for (let col = 0; col < cols; col++) {
      const offset = row % 2 === 0 ? 0 : 0.5
      // Create realistic-looking distribution with higher activity in center
      const centerX = cols / 2
      const centerY = rows / 2
      const distFromCenter = Math.sqrt(
        Math.pow(col - centerX, 2) + Math.pow(row - centerY, 2)
      )
      const baseDensity = Math.max(0, 1 - distFromCenter / 8)
      const randomFactor = 0.3 + Math.random() * 0.7
      const trips = Math.floor(baseDensity * randomFactor * 500)
      const avgFare = 15 + (baseDensity * 25) + (Math.random() * 10)
      
      hexes.push({
        id: `hex-${row}-${col}`,
        x: col + offset,
        y: row,
        trips,
        avgFare: Math.round(avgFare * 100) / 100,
        zone: zones[Math.floor(Math.random() * zones.length)],
      })
    }
  }
  
  return hexes
}

function getHexColor(avgFare: number): string {
  // Color scale using indigo/violet for primary, emerald for low, coral for high demand
  if (avgFare < 20) return 'rgba(16, 185, 129, 0.6)' // Emerald (success/low)
  if (avgFare < 25) return 'rgba(99, 102, 241, 0.65)' // Indigo
  if (avgFare < 30) return 'rgba(99, 102, 241, 0.75)' // Indigo (brighter)
  if (avgFare < 35) return 'rgba(249, 112, 102, 0.8)' // Coral (urgent)
  return 'rgba(249, 112, 102, 0.9)' // Coral (high urgency)
}

function getHexHeight(trips: number): number {
  // Height based on trip volume (3D effect)
  return Math.min(trips / 10, 30)
}

interface HexagonProps {
  cell: HexCell
  size: number
  onSelect: (cell: HexCell) => void
  isSelected: boolean
}

function Hexagon({ cell, size, onSelect, isSelected }: HexagonProps) {
  const [isHovered, setIsHovered] = React.useState(false)
  
  const width = size * 2
  const height = size * Math.sqrt(3)
  const x = cell.x * width * 0.75
  const y = cell.y * height
  
  const hexHeight = getHexHeight(cell.trips)
  const color = getHexColor(cell.avgFare)
  
  // Create hexagon path
  const points = Array.from({ length: 6 }, (_, i) => {
    const angle = (Math.PI / 3) * i - Math.PI / 6
    return `${size + size * Math.cos(angle)},${size * Math.sqrt(3) / 2 + size * Math.sin(angle)}`
  }).join(' ')

  return (
    <ContextMenu>
      <ContextMenuTrigger asChild>
        <g
          transform={`translate(${x}, ${y})`}
          className="cursor-pointer transition-transform duration-200"
          style={{
            transform: `translate(${x}px, ${y}px) ${isHovered ? 'scale(1.05)' : 'scale(1)'}`,
          }}
          onMouseEnter={() => setIsHovered(true)}
          onMouseLeave={() => setIsHovered(false)}
          onClick={() => onSelect(cell)}
        >
          {/* 3D effect - side faces */}
          {hexHeight > 0 && (
            <>
              <polygon
                points={`${size},0 ${size * 1.5},${size * Math.sqrt(3) / 4} ${size * 1.5},${size * Math.sqrt(3) / 4 + hexHeight} ${size},${hexHeight}`}
                fill="rgba(0,0,0,0.3)"
                className="pointer-events-none"
              />
              <polygon
                points={`${size * 1.5},${size * Math.sqrt(3) / 4} ${size * 1.5},${size * Math.sqrt(3) * 3 / 4} ${size * 1.5},${size * Math.sqrt(3) * 3 / 4 + hexHeight} ${size * 1.5},${size * Math.sqrt(3) / 4 + hexHeight}`}
                fill="rgba(0,0,0,0.2)"
                className="pointer-events-none"
              />
            </>
          )}
          
          {/* Main hex face */}
          <polygon
            points={points}
            fill={color}
            stroke={isSelected ? 'rgb(168, 85, 247)' : isHovered ? 'rgba(255,255,255,0.5)' : 'rgba(255,255,255,0.1)'}
            strokeWidth={isSelected ? 2 : 1}
            style={{
              filter: isHovered ? 'brightness(1.2)' : 'none',
              transform: `translateY(-${hexHeight}px)`,
            }}
            className="transition-all duration-200"
          />
          
          {/* Glow effect for high-traffic areas */}
          {cell.trips > 300 && (
            <polygon
              points={points}
              fill="none"
              stroke="rgba(99, 102, 241, 0.5)"
              strokeWidth={2}
              style={{
                filter: 'blur(4px)',
                transform: `translateY(-${hexHeight}px)`,
              }}
              className="pointer-events-none animate-pulse"
            />
          )}
        </g>
      </ContextMenuTrigger>
      <ContextMenuContent className="w-48 bg-popover/95 backdrop-blur-sm border-primary/20">
        <ContextMenuItem className="gap-2 text-primary hover:bg-primary/10">
          <Send className="size-4" />
          Dispatch Drivers
        </ContextMenuItem>
        <ContextMenuItem className="gap-2 hover:bg-primary/10">
          <BarChart3 className="size-4" />
          View Zone Analytics
        </ContextMenuItem>
        <ContextMenuItem className="gap-2 hover:bg-primary/10">
          <Bell className="size-4" />
          Send Alert
        </ContextMenuItem>
      </ContextMenuContent>
    </ContextMenu>
  )
}

interface HexMapTooltipProps {
  cell: HexCell | null
  position: { x: number; y: number }
}

function HexMapTooltip({ cell, position }: HexMapTooltipProps) {
  if (!cell) return null
  
  return (
    <div
      className="absolute pointer-events-none z-50 bg-popover/95 backdrop-blur-sm border border-primary/30 rounded-lg px-3 py-2 shadow-lg shadow-primary/10"
      style={{
        left: position.x + 10,
        top: position.y + 10,
      }}
    >
      <div className="flex flex-col gap-1">
        <div className="text-xs text-muted-foreground">Zone: <span className="text-foreground font-medium">{cell.zone}</span></div>
        <div className="text-xs text-muted-foreground">Trips: <span className="text-foreground font-medium">{cell.trips}</span></div>
        <div className="text-xs text-muted-foreground">Avg Fare: <span className="text-primary font-medium">${cell.avgFare}</span></div>
      </div>
    </div>
  )
}

interface LayerControlProps {
  layers: {
    demand: boolean
    traffic: boolean
    drivers: boolean
  }
  opacity: number
  onLayerToggle: (layer: 'demand' | 'traffic' | 'drivers') => void
  onOpacityChange: (value: number) => void
}

function LayerControl({ layers, opacity, onLayerToggle, onOpacityChange }: LayerControlProps) {
  const [isExpanded, setIsExpanded] = React.useState(true)

  return (
    <div className="absolute top-4 right-4 z-20 bg-card/80 backdrop-blur-xl border border-border/50 rounded-lg shadow-lg shadow-primary/5 w-56">
      {/* Header */}
      <button
        onClick={() => setIsExpanded(!isExpanded)}
        className="w-full flex items-center justify-between px-3 py-2.5 text-sm font-medium text-foreground hover:bg-muted/50 rounded-t-lg transition-colors"
      >
        <div className="flex items-center gap-2">
          <Layers className="size-4 text-primary" />
          <span>Layer Controls</span>
        </div>
        {isExpanded ? (
          <ChevronUp className="size-4 text-muted-foreground" />
        ) : (
          <ChevronDown className="size-4 text-muted-foreground" />
        )}
      </button>

      {isExpanded && (
        <div className="px-3 pb-3 space-y-3">
          {/* Layer toggles */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <label className="text-xs text-muted-foreground flex items-center gap-2">
                {layers.demand ? <Eye className="size-3" /> : <EyeOff className="size-3" />}
                Demand Heatmap
              </label>
              <Switch
                checked={layers.demand}
                onCheckedChange={() => onLayerToggle('demand')}
                className="scale-75"
              />
            </div>
            <div className="flex items-center justify-between">
              <label className="text-xs text-muted-foreground flex items-center gap-2">
                {layers.traffic ? <Eye className="size-3" /> : <EyeOff className="size-3" />}
                Traffic Flow
              </label>
              <Switch
                checked={layers.traffic}
                onCheckedChange={() => onLayerToggle('traffic')}
                className="scale-75"
              />
            </div>
            <div className="flex items-center justify-between">
              <label className="text-xs text-muted-foreground flex items-center gap-2">
                {layers.drivers ? <Eye className="size-3" /> : <EyeOff className="size-3" />}
                Active Drivers
              </label>
              <Switch
                checked={layers.drivers}
                onCheckedChange={() => onLayerToggle('drivers')}
                className="scale-75"
              />
            </div>
          </div>

          {/* Opacity slider */}
          <div className="space-y-1.5 pt-2 border-t border-border/50">
            <div className="flex items-center justify-between">
              <label className="text-xs text-muted-foreground">Opacity</label>
              <span className="text-xs text-foreground font-medium">{Math.round(opacity * 100)}%</span>
            </div>
            <Slider
              value={[opacity * 100]}
              onValueChange={(value) => onOpacityChange(value[0] / 100)}
              min={20}
              max={100}
              step={5}
              className="py-1"
            />
          </div>
        </div>
      )}
    </div>
  )
}

interface HexMapProps {
  onSelectZone?: (cell: HexCell | null) => void
  selectedZone?: HexCell | null
}

export function HexMap({ onSelectZone, selectedZone }: HexMapProps) {
  const [hexes] = React.useState(() => generateHexGrid())
  const [hoveredCell, setHoveredCell] = React.useState<HexCell | null>(null)
  const [mousePos, setMousePos] = React.useState({ x: 0, y: 0 })
  const containerRef = React.useRef<HTMLDivElement>(null)
  const [layers, setLayers] = React.useState({
    demand: true,
    traffic: true,
    drivers: true,
  })
  const [opacity, setOpacity] = React.useState(0.85)
  
  const hexSize = 28
  
  const handleLayerToggle = (layer: 'demand' | 'traffic' | 'drivers') => {
    setLayers((prev) => ({ ...prev, [layer]: !prev[layer] }))
  }
  
  const handleMouseMove = (e: React.MouseEvent) => {
    if (containerRef.current) {
      const rect = containerRef.current.getBoundingClientRect()
      setMousePos({ x: e.clientX - rect.left, y: e.clientY - rect.top })
    }
  }
  
  return (
    <div 
      ref={containerRef}
      className="relative w-full h-full overflow-hidden bg-gradient-to-br from-background via-card to-background rounded-lg"
      onMouseMove={handleMouseMove}
    >
      {/* Grid overlay effect */}
      <div className="absolute inset-0 bg-[linear-gradient(rgba(99,102,241,0.03)_1px,transparent_1px),linear-gradient(90deg,rgba(99,102,241,0.03)_1px,transparent_1px)] bg-[size:50px_50px] pointer-events-none" />

      {/* Floating Layer Control */}
      <LayerControl
        layers={layers}
        opacity={opacity}
        onLayerToggle={handleLayerToggle}
        onOpacityChange={setOpacity}
      />
      
      {/* Map title */}
      <div className="absolute top-4 left-4 z-10">
        <h2 className="text-lg font-semibold text-foreground">New York City</h2>
        <p className="text-sm text-muted-foreground">H3 Hexagonal Grid Visualization</p>
      </div>
      
      {/* Legend */}
      <div className="absolute bottom-4 left-4 z-10 bg-card/80 backdrop-blur-sm border border-border/50 rounded-lg p-3">
        <div className="text-xs text-muted-foreground mb-2 font-medium">Legend</div>
        <div className="flex flex-col gap-1.5">
          <div className="flex items-center gap-2">
            <div className="text-[10px] text-muted-foreground w-16">Height</div>
            <div className="flex items-end gap-0.5 h-4">
              {[1, 2, 3, 4, 5].map((h) => (
                <div key={h} className="w-2 bg-primary/60 rounded-t-sm" style={{ height: h * 3 }} />
              ))}
            </div>
            <div className="text-[10px] text-muted-foreground">Trip Volume</div>
          </div>
          <div className="flex items-center gap-2">
            <div className="text-[10px] text-muted-foreground w-16">Color</div>
            <div className="flex gap-0.5">
              {['rgba(16, 185, 129, 0.8)', 'rgba(99, 102, 241, 0.7)', 'rgba(99, 102, 241, 0.85)', 'rgba(249, 112, 102, 0.8)', 'rgba(249, 112, 102, 0.95)'].map((color, i) => (
                <div key={i} className="w-4 h-4 rounded-sm" style={{ backgroundColor: color }} />
              ))}
            </div>
            <div className="text-[10px] text-muted-foreground">Avg Fare</div>
          </div>
        </div>
      </div>
      
      {/* SVG Map */}
      <svg
        width="100%"
        height="100%"
        viewBox="0 0 750 450"
        preserveAspectRatio="xMidYMid meet"
        className="absolute inset-0"
      >
        <defs>
          <filter id="glow">
            <feGaussianBlur stdDeviation="3" result="coloredBlur" />
            <feMerge>
              <feMergeNode in="coloredBlur" />
              <feMergeNode in="SourceGraphic" />
            </feMerge>
          </filter>
        </defs>
        
        <g transform="translate(50, 30)" style={{ opacity: layers.demand ? opacity : 0.2 }}>
          {hexes.map((cell) => (
            <Hexagon
              key={cell.id}
              cell={cell}
              size={hexSize}
              onSelect={(c) => {
                onSelectZone?.(c)
                setHoveredCell(c)
              }}
              isSelected={selectedZone?.id === cell.id}
            />
          ))}
        </g>
      </svg>
      
      {/* Tooltip */}
      <HexMapTooltip cell={hoveredCell} position={mousePos} />
    </div>
  )
}

export type { HexCell }
