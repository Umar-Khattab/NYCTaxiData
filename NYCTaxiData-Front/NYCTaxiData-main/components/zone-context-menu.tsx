'use client'

import * as React from 'react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { Send, X, ClipboardList, Users } from 'lucide-react'

const MENU_WIDTH = 248
const MENU_HEIGHT = 220
const OFFSET = 12

export interface ZoneContextMenuProps {
  open: boolean
  position: { x: number; y: number }
  zoneName: string
  zoneId: string
  activeRequestCount: number
  availableDriverCount: number
  onDispatch: (zoneId: string) => void
  onClose: () => void
  className?: string
}

function clampMenuPosition(
  x: number,
  y: number,
  containerWidth: number,
  containerHeight: number
) {
  const left = Math.min(Math.max(OFFSET, x), containerWidth - MENU_WIDTH - OFFSET)
  const top = Math.min(Math.max(OFFSET, y), containerHeight - MENU_HEIGHT - OFFSET)
  return { left, top }
}

export function ZoneContextMenu({
  open,
  position,
  zoneName,
  zoneId,
  activeRequestCount,
  availableDriverCount,
  onDispatch,
  onClose,
  className,
}: ZoneContextMenuProps) {
  const menuRef = React.useRef<HTMLDivElement>(null)
  const [clamped, setClamped] = React.useState({ left: position.x, top: position.y })

  React.useLayoutEffect(() => {
    if (!open) return
    const parent = menuRef.current?.offsetParent as HTMLElement | null
    const w = parent?.clientWidth ?? window.innerWidth
    const h = parent?.clientHeight ?? window.innerHeight
    setClamped(clampMenuPosition(position.x, position.y, w, h))
  }, [open, position.x, position.y])

  React.useEffect(() => {
    if (!open) return

    const handlePointerDown = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        onClose()
      }
    }

    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }

    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleEscape)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleEscape)
    }
  }, [open, onClose])

  if (!open) return null

  return (
    <div
      ref={menuRef}
      role="menu"
      aria-label={`Zone actions for ${zoneName}`}
      style={{ left: clamped.left, top: clamped.top }}
      className={cn(
        'absolute z-50 w-[248px] pointer-events-auto',
        'glass-cyber glow-purple rounded-xl border border-[#7C3AED]/35',
        'bg-[#0d0e15]/92 backdrop-blur-xl shadow-[0_12px_48px_rgba(0,0,0,0.55),0_0_24px_rgba(124,58,237,0.15)]',
        'animate-in fade-in zoom-in-95 duration-200 ease-out',
        className
      )}
    >
      <div className="h-0.5 w-full rounded-t-xl bg-gradient-to-r from-[#7C3AED] via-[#A78BFA] to-[#38BDF8]" />

      <div className="p-3.5">
        <div className="flex items-start justify-between gap-2 mb-3">
          <div className="min-w-0">
            <p className="text-[9px] font-mono font-bold uppercase tracking-widest text-[#A78BFA] mb-0.5">
              Zone Selected
            </p>
            <h3 className="text-sm font-black text-foreground leading-tight truncate">
              {zoneName}
            </h3>
            <p className="text-[9px] font-mono text-muted-foreground truncate mt-0.5">
              {zoneId.slice(-8).toUpperCase()}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="shrink-0 rounded-md p-1 text-muted-foreground hover:text-foreground hover:bg-white/5 transition-colors"
            aria-label="Close menu"
          >
            <X className="size-3.5" />
          </button>
        </div>

        <div className="grid grid-cols-2 gap-2 mb-3">
          <div className="rounded-lg border border-[#7C3AED]/20 bg-[#7C3AED]/8 px-2.5 py-2">
            <div className="flex items-center gap-1.5 text-muted-foreground mb-1">
              <ClipboardList className="size-3 text-[#A78BFA]" />
              <span className="text-[9px] font-mono uppercase tracking-wider">Requests</span>
            </div>
            <p className="text-lg font-black font-mono text-[#A78BFA] tabular-nums">
              {activeRequestCount}
            </p>
          </div>
          <div className="rounded-lg border border-[#38BDF8]/20 bg-[#38BDF8]/8 px-2.5 py-2">
            <div className="flex items-center gap-1.5 text-muted-foreground mb-1">
              <Users className="size-3 text-[#38BDF8]" />
              <span className="text-[9px] font-mono uppercase tracking-wider">Drivers</span>
            </div>
            <p className="text-lg font-black font-mono text-[#38BDF8] tabular-nums">
              {availableDriverCount}
            </p>
          </div>
        </div>

        <Button
          type="button"
          size="sm"
          className="w-full h-10 gap-2 bg-primary hover:bg-primary/90 glow-purple font-bold text-sm shadow-[0_0_20px_rgba(124,58,237,0.35)]"
          onClick={() => onDispatch(zoneId)}
        >
          <Send className="size-4" />
          Dispatch
        </Button>
      </div>
    </div>
  )
}
