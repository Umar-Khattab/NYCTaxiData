'use client'

import * as React from 'react'
import { CommandMap } from '@/components/command-map'
import { ZoneContextMenu } from '@/components/zone-context-menu'
import { useDispatchStore } from '@/stores/use-dispatch-store'
import type { MapZoneSelection, ZoneMapClickEvent } from '@/lib/dispatch-types'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { MapPin, RotateCcw, MousePointerClick, Crosshair } from 'lucide-react'

export interface DemandHeatmapProps {
  className?: string
  showFleet?: boolean
  showGap?: boolean
}

export function DemandHeatmap({
  className,
  showFleet = true,
  showGap = false,
}: DemandHeatmapProps) {
  const mapContainerRef = React.useRef<HTMLDivElement>(null)

  const selectedZoneId = useDispatchStore((s) => s.selectedZoneId)
  const selectedZoneLabel = useDispatchStore((s) => s.selectedZoneLabel)
  const selectedZoneMetrics = useDispatchStore((s) => s.selectedZoneMetrics)
  const setSelectedZone = useDispatchStore((s) => s.setSelectedZone)
  const clearSelectedZone = useDispatchStore((s) => s.clearSelectedZone)
  const dispatchZone = useDispatchStore((s) => s.dispatchZone)

  const [contextMenu, setContextMenu] = React.useState<{
    open: boolean
    position: { x: number; y: number }
    zone: MapZoneSelection
  } | null>(null)

  const isFocusMode = Boolean(selectedZoneId)

  const handleZoneMapClick = React.useCallback((event: ZoneMapClickEvent) => {
    setContextMenu({
      open: true,
      position: event.position,
      zone: event.zone,
    })
  }, [])

  const handleSelectZone = React.useCallback(
    (zone: MapZoneSelection) => setSelectedZone(zone),
    [setSelectedZone]
  )

  const handleDispatchFromMenu = React.useCallback(
    (zoneId: string) => {
      const zone =
        contextMenu?.zone.zoneId === zoneId
          ? contextMenu.zone
          : selectedZoneMetrics?.zoneId === zoneId
            ? selectedZoneMetrics
            : null

      if (zone) {
        dispatchZone(zone)
      }
      setContextMenu(null)
    },
    [contextMenu, selectedZoneMetrics, dispatchZone]
  )

  const closeContextMenu = React.useCallback(() => setContextMenu(null), [])

  return (
    <div
      ref={mapContainerRef}
      className={cn(
        'relative h-full w-full min-h-[320px] transition-opacity duration-500',
        className
      )}
    >
      <CommandMap
        embedded
        showDemand={!showGap}
        showGap={showGap}
        showFleet={showFleet}
        highlightedZoneId={selectedZoneId}
        focusZoneOnSelect
        onZoneMapClick={handleZoneMapClick}
        onSelectZone={handleSelectZone}
        className="h-full rounded-none border-0 shadow-none"
      />

      {contextMenu?.open && (
        <ZoneContextMenu
          open
          position={contextMenu.position}
          zoneName={contextMenu.zone.zone}
          zoneId={contextMenu.zone.zoneId}
          activeRequestCount={contextMenu.zone.trips}
          availableDriverCount={contextMenu.zone.supply}
          onDispatch={handleDispatchFromMenu}
          onClose={closeContextMenu}
        />
      )}

      <div className="absolute top-20 left-4 z-30 flex flex-col gap-2 max-w-[300px] pointer-events-none">
        {isFocusMode ? (
          <div className="glass-cyber glow-purple rounded-xl p-3 pointer-events-auto border-[#A78BFA]/40 animate-in fade-in slide-in-from-left-2 duration-300">
            <div className="flex items-center gap-2 mb-2">
              <Crosshair className="size-3.5 text-[#A78BFA] animate-pulse" />
              <span className="text-[9px] font-mono font-bold uppercase tracking-widest text-[#A78BFA]">
                Focus Mode Active
              </span>
            </div>
            <p className="text-sm font-black text-foreground truncate">{selectedZoneLabel}</p>
            <p className="text-[10px] font-mono text-muted-foreground truncate mb-2">
              {selectedZoneId?.slice(-8).toUpperCase()}
            </p>
            {selectedZoneMetrics && (
              <div className="grid grid-cols-2 gap-2 text-[10px] font-mono mb-3">
                <div>
                  <span className="text-muted-foreground">Demand</span>
                  <p className="font-bold text-[#A78BFA]">{selectedZoneMetrics.trips}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Fleet</span>
                  <p className="font-bold text-[#38BDF8]">{selectedZoneMetrics.supply}</p>
                </div>
              </div>
            )}
            <Button
              type="button"
              size="sm"
              variant="outline"
              className="w-full h-8 gap-2 border-primary/40 text-[#A78BFA] hover:bg-primary/15 text-xs"
              onClick={() => {
                closeContextMenu()
                clearSelectedZone()
              }}
            >
              <RotateCcw className="size-3.5" />
              Reset City View
            </Button>
          </div>
        ) : (
          <div className="glass-cyber rounded-xl px-3 py-2 flex items-center gap-2 text-[10px] text-muted-foreground">
            <MousePointerClick className="size-3.5 text-[#7C3AED] shrink-0" />
            <span>Click a zone for actions — dispatch, fleet & queue sync</span>
          </div>
        )}
      </div>

      <div className="absolute bottom-4 left-4 z-30 glass-cyber rounded-xl p-3 pointer-events-none w-52">
        <p className="text-[9px] font-mono font-bold uppercase tracking-widest text-primary mb-2">
          Passenger Demand
        </p>
        <div className="h-2 w-full rounded-full bg-gradient-to-r from-[#201834] via-[#7C3AED] via-50% to-[#F87171] border border-white/[0.06]" />
        <div className="flex justify-between mt-1.5 text-[8px] uppercase font-bold tracking-wider text-muted-foreground">
          <span>Low</span>
          <span className="text-[#F87171]">Peak</span>
        </div>
      </div>

      <div className="absolute bottom-4 right-4 z-30 pointer-events-none">
        <div
          className={cn(
            'glass-cyber rounded-full px-3 py-1.5 flex items-center gap-1.5 text-[10px] font-mono transition-all duration-300',
            isFocusMode ? 'text-[#A78BFA] glow-purple border-primary/30' : 'text-[#38BDF8] glow-cyan'
          )}
        >
          <MapPin className="size-3" />
          {isFocusMode ? 'Zone Focus' : 'H3 · Res 8'}
        </div>
      </div>
    </div>
  )
}
