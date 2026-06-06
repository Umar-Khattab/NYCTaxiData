'use client'

import * as React from 'react'
import { AlertTriangle, CheckCircle, Info, X, Clock } from 'lucide-react'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { fleetApi, type ApiLogEntry } from '@/lib/fleet-api'

interface Alert {
  id: string
  type: 'success' | 'warning' | 'info' | 'error'
  message: string
  timestamp: Date
}

function apiLogToAlert(log: ApiLogEntry): Alert {
  return {
    id: log.id,
    type: log.type,
    message: log.message,
    timestamp: new Date(log.timestamp),
  }
}

function getAlertIcon(type: Alert['type']) {
  switch (type) {
    case 'success':
      return CheckCircle
    case 'warning':
      return AlertTriangle
    case 'error':
      return AlertTriangle
    default:
      return Info
  }
}

function getAlertStyles(type: Alert['type']) {
  switch (type) {
    case 'success':
      return 'text-[#4ADE80] bg-[#4ADE80]/10 border-[#4ADE80]/20 shadow-[0_0_8px_rgba(74,222,128,0.15)]'
    case 'warning':
      return 'text-[#FB923C] bg-[#FB923C]/10 border-[#FB923C]/20 shadow-[0_0_8px_rgba(251,146,60,0.15)]'
    case 'error':
      return 'text-[#F87171] bg-[#F87171]/10 border-[#F87171]/20 shadow-[0_0_8px_rgba(248,113,113,0.15)]'
    default:
      return 'text-[#A78BFA] bg-[#A78BFA]/10 border-[#A78BFA]/20 shadow-[0_0_8px_rgba(167,139,250,0.15)]'
  }
}

function formatTimestamp(date: Date) {
  const diff = Date.now() - date.getTime()
  const seconds = Math.floor(diff / 1000)
  const minutes = Math.floor(seconds / 60)
  
  if (minutes < 1) return 'Just now'
  if (minutes < 60) return `${minutes}m ago`
  return `${Math.floor(minutes / 60)}h ago`
}

export function StatusBar() {
  const [mounted, setMounted] = React.useState(false)
  const [alerts, setAlerts] = React.useState<Alert[]>([])
  const [currentTime, setCurrentTime] = React.useState(new Date())

  React.useEffect(() => {
    setMounted(true)
  }, [])

  // Live clock
  React.useEffect(() => {
    const id = setInterval(() => setCurrentTime(new Date()), 1000)
    return () => clearInterval(id)
  }, [])

  // Poll logs from API every 15 s
  React.useEffect(() => {
    let cancelled = false
    const load = async () => {
      try {
        const logs = await fleetApi.logs.getAll()
        if (cancelled) return
        // Show the 5 most recent, newest first
        const recent = logs
          .slice()
          .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime())
          .slice(0, 5)
          .map(apiLogToAlert)
        setAlerts(recent)
      } catch {
        // Silent — keep displaying whatever we have
      }
    }
    load()
    const id = setInterval(load, 15_000)
    return () => { cancelled = true; clearInterval(id) }
  }, [])

  const dismissAlert = (id: string) => {
    setAlerts((prev) => prev.filter((alert) => alert.id !== id))
  }

  return (
    <div className="h-12 bg-card/75 backdrop-blur-[12px] border-t border-border flex items-center px-4 gap-4 select-none theme-transition">
      {/* Connection Status */}
      <div className="flex items-center gap-2 pr-4 border-r border-border">
        <div className="flex items-center gap-1.5 px-2 py-0.5 bg-success/15 text-success border border-success/30 rounded-lg text-xs font-mono font-bold glow-green">
          <span className="w-1.5 h-1.5 rounded-full bg-success animate-pulse" />
          CONNECTED
        </div>
      </div>

      {/* Current Time - Glowing Digital Clock */}
      <div className="flex items-center gap-2 pr-4 border-r border-border">
        <Clock className="size-3.5 text-accent filter drop-shadow-[0_0_4px_var(--accent)]" />
        <span className="text-xs font-mono font-bold text-accent glow-lavender">
          {mounted ? currentTime.toLocaleTimeString('en-US', { hour12: false }) : '--:--:--'}
        </span>
      </div>

      {/* Scrolling Alerts */}
      <div className="flex-1 overflow-hidden">
        <div className="flex items-center gap-3 animate-marquee">
          {alerts.map((alert) => {
            const Icon = getAlertIcon(alert.type)
            return (
              <div
                key={alert.id}
                className={cn(
                  'flex items-center gap-2 px-3 py-1 rounded-full border text-xs whitespace-nowrap transition-all duration-300 font-mono font-medium',
                  getAlertStyles(alert.type)
                )}
              >
                <Icon className="size-3" />
                <span>{alert.message}</span>
                <span className="text-[9px] opacity-60 font-bold">{formatTimestamp(alert.timestamp)}</span>
                <Button
                  variant="ghost"
                  size="icon"
                  className="size-4 p-0 hover:bg-transparent text-inherit opacity-50 hover:opacity-100"
                  onClick={() => dismissAlert(alert.id)}
                >
                  <X className="size-3" />
                </Button>
              </div>
            )
          })}
        </div>
      </div>

      {/* Alert Count */}
      <div className="flex items-center gap-2 pl-4 border-l border-border">
        <div className="flex items-center gap-1.5 text-xs text-muted-foreground font-mono">
          <span className="font-bold text-foreground bg-secondary border border-border px-1.5 py-0.5 rounded">{alerts.length}</span>
          <span>SYSTEM LOGS</span>
        </div>
      </div>
    </div>
  )
}
