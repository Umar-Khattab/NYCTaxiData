'use client'

import * as React from 'react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { ChevronLeft, ChevronRight, ListOrdered } from 'lucide-react'

interface DispatchSidePanelProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  title?: string
  subtitle?: string
  children: React.ReactNode
  className?: string
}

export function DispatchSidePanel({
  open,
  onOpenChange,
  title = 'Dispatch Queue',
  subtitle,
  children,
  className,
}: DispatchSidePanelProps) {
  return (
    <>
      {!open && (
        <Button
          type="button"
          onClick={() => onOpenChange(true)}
          className={cn(
            'absolute right-0 top-1/2 z-40 -translate-y-1/2 rounded-l-xl rounded-r-none',
            'h-auto py-6 px-2 flex flex-col gap-2',
            'glass-cyber border border-r-0 border-primary/30 text-[#A78BFA] hover:bg-primary/10 glow-purple'
          )}
          aria-label="Open dispatch panel"
        >
          <ListOrdered className="size-4" />
          <ChevronLeft className="size-4" />
        </Button>
      )}

      <div
        className={cn(
          'absolute top-3 bottom-3 right-3 z-40 flex flex-col',
          'transition-[transform,opacity] duration-300 ease-out',
          open ? 'translate-x-0 opacity-100' : 'translate-x-[calc(100%+1rem)] opacity-0 pointer-events-none',
          className
        )}
      >
        <div className="glass-cyber glow-purple flex h-full w-[min(100vw-1.5rem,420px)] flex-col overflow-hidden rounded-xl border border-primary/25 shadow-2xl">
          <div className="flex items-center justify-between gap-2 border-b border-border/60 px-4 py-3 shrink-0">
            <div className="min-w-0">
              <h2 className="text-sm font-bold text-foreground tracking-tight">{title}</h2>
              {subtitle && (
                <p className="text-[10px] text-muted-foreground truncate">{subtitle}</p>
              )}
            </div>
            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="size-8 shrink-0 text-muted-foreground hover:text-foreground"
              onClick={() => onOpenChange(false)}
              aria-label="Collapse dispatch panel"
            >
              <ChevronRight className="size-4" />
            </Button>
          </div>
          <div className="flex-1 min-h-0 overflow-hidden flex flex-col">{children}</div>
        </div>
      </div>
    </>
  )
}
