'use client'

import * as React from 'react'
import Link from 'next/link'
import { usePathname } from 'next/navigation'
import {
  Radar,
  MapPin,
  BrainCircuit,
  Megaphone,
  BarChart3,
  ShieldAlert,
  ChevronLeft,
  Car,
  SplitSquareHorizontal,
} from 'lucide-react'

import { cn } from '@/lib/utils'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Sidebar,
  SidebarContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuItem,
  SidebarMenuButton,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  useSidebar,
} from '@/components/ui/sidebar'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'

const navItems = [
  {
    title: 'Prediction Dashboard',
    href: '/',
    icon: Radar,
    badge: 'LIVE',
    badgeVariant: 'live' as const,
  },
  {
    title: 'Driver Analytics',
    href: '/fleet',
    icon: MapPin,
    badge: 'Live',
    badgeVariant: 'live' as const,
  },
  {
    title: 'Simulation Heatmap',
    href: '/analytics',
    icon: BrainCircuit,
    badge: 'AI',
    badgeVariant: 'ai' as const,
  },
  {
    title: 'Driver Operations',
    href: '/dispatch',
    icon: Megaphone,
    badge: 'Map + Queue',
    badgeVariant: 'action' as const,
  },
  {
    title: 'System Logs & Alerts',
    href: '/logs',
    icon: ShieldAlert,
    badge: 'Alerts',
    badgeVariant: 'alerts' as const,
  },
]

function getBadgeStyles(variant: string) {
  switch (variant) {
    case 'live':
      return 'bg-primary/20 text-primary border-primary/30 animate-pulse'
    case 'ai':
      return 'bg-chart-2/20 text-chart-2 border-chart-2/30'
    case 'action':
      return 'bg-chart-3/20 text-chart-3 border-chart-3/30'
    case 'insights':
      return 'bg-chart-5/20 text-chart-5 border-chart-5/30'
    case 'alerts':
      return 'bg-destructive/20 text-destructive border-destructive/30'
    default:
      return 'bg-muted text-muted-foreground border-border'
  }
}

export function AppSidebar() {
  const pathname = usePathname()
  const { state, toggleSidebar } = useSidebar()
  const isCollapsed = state === 'collapsed'
  const [authorized, setAuthorized] = React.useState<boolean | null>(null)
  const [progress, setProgress] = React.useState(0)
  const [phase, setPhase] = React.useState<"loading" | "fadeout">("loading")

  React.useEffect(() => {
    const loggedIn = localStorage.getItem("isLoggedIn") === "true"
    if (!loggedIn) {
      window.location.href = "/login"
      return
    }

    // Check if loader has already been completed in this session
    const hasLoaded = sessionStorage.getItem("hasLoadedDashboard") === "true"
    if (hasLoaded) {
      setAuthorized(true)
      return
    }

    // Fast loading animation for credentials verification
    const startTime = Date.now()
    const duration = 1200 // 1.2 seconds

    const interval = setInterval(() => {
      const elapsed = Date.now() - startTime
      const pct = Math.min((elapsed / duration) * 100, 100)
      setProgress(pct)

      if (pct >= 100) {
        clearInterval(interval)
        setPhase("fadeout")
        sessionStorage.setItem("hasLoadedDashboard", "true")
        setTimeout(() => {
          setAuthorized(true)
        }, 300) // matches fade-out transition
      }
    }, 16)

    return () => clearInterval(interval)
  }, [])

  if (authorized === null) {
    return (
      <div
        className="fixed inset-0 z-50 flex flex-col items-center justify-center overflow-hidden"
        style={{
          background: "#04020a",
          transition: "opacity 0.3s ease",
          opacity: phase === "fadeout" ? 0 : 1,
          pointerEvents: phase === "fadeout" ? "none" : "all",
        }}
      >
        {/* Radial vignette */}
        <div
          className="absolute inset-0"
          style={{
            background:
              "radial-gradient(ellipse at center, rgba(124,58,237,0.08) 0%, rgba(4,2,10,0.7) 70%)",
          }}
        />

        {/* Content */}
        <div className="relative z-10 flex flex-col items-center w-full max-w-md px-8">
          {/* Logo */}
          <div className="flex flex-col items-center mb-8">
            <div
              className="w-16 h-16 rounded-2xl flex items-center justify-center mb-3 animate-pulse"
              style={{
                background: "linear-gradient(135deg, rgba(124,58,237,0.2), rgba(79,70,229,0.1))",
                border: "1px solid rgba(124,58,237,0.4)",
                boxShadow: "0 0 40px rgba(124,58,237,0.3), inset 0 0 20px rgba(124,58,237,0.05)",
              }}
            >
              <svg viewBox="0 0 24 24" fill="none" className="w-8 h-8 text-violet-400" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" style={{ filter: "drop-shadow(0 0 8px rgba(124,58,237,0.8))" }}>
                <rect x="2" y="11" width="20" height="8" rx="2"/>
                <path d="M5 11V9a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v2"/>
                <path d="M7 7l1.5-3h7L17 7"/>
                <circle cx="7" cy="19" r="1.5" fill="currentColor"/>
                <circle cx="17" cy="19" r="1.5" fill="currentColor"/>
                <path d="M10 8h4"/>
              </svg>
            </div>
            <div
              className="text-xl font-bold tracking-[0.3em] uppercase"
              style={{
                background: "linear-gradient(90deg, #8b5cf6, #a78bfa, #06b6d4)",
                WebkitBackgroundClip: "text",
                WebkitTextFillColor: "transparent",
                filter: "drop-shadow(0 0 6px rgba(124,58,237,0.3))",
              }}
            >
              FleetCommand
            </div>
            <div className="text-[9px] tracking-[0.4em] text-violet-500/80 uppercase mt-0.5">
              Secure Session Manager
            </div>
          </div>

          {/* Verification Steps Terminal */}
          <div
            className="w-full rounded-xl mb-5 p-4 font-mono text-[11px] overflow-hidden"
            style={{
              background: "rgba(6,3,13,0.85)",
              border: "1px solid rgba(124,58,237,0.2)",
              minHeight: 90,
            }}
          >
            <div className="flex items-center gap-2 mb-1">
              <span className="text-emerald-500">✓</span>
              <span className="text-violet-400/60">INITIALIZING SECURE SESSION...</span>
            </div>
            {progress >= 35 && (
              <div className="flex items-center gap-2 mb-1">
                <span className={progress >= 70 ? "text-emerald-500" : "text-cyan-400 animate-pulse"}>
                  {progress >= 70 ? "✓" : "▶"}
                </span>
                <span className={progress >= 70 ? "text-violet-400/60" : "text-cyan-300 font-bold"}>
                  VERIFYING SECURE CREDENTIALS...
                </span>
              </div>
            )}
            {progress >= 70 && (
              <div className="flex items-center gap-2">
                <span className={progress >= 100 ? "text-emerald-500" : "text-cyan-400 animate-pulse"}>
                  {progress >= 100 ? "✓" : "▶"}
                </span>
                <span className={progress >= 100 ? "text-violet-400/60" : "text-cyan-300 font-bold"}>
                  DECRYPTING TELEMETRY GRID...
                </span>
              </div>
            )}
          </div>

          {/* Progress bar */}
          <div className="w-full">
            <div className="flex justify-between items-center mb-1.5">
              <span className="text-[9px] text-violet-500 font-mono tracking-widest uppercase">
                Session Verification
              </span>
              <span className="text-[9px] text-cyan-400 font-mono font-bold">
                {Math.round(progress)}%
              </span>
            </div>
            <div
              className="w-full h-1 rounded-full overflow-hidden"
              style={{ background: "rgba(124,58,237,0.1)", border: "1px solid rgba(124,58,237,0.15)" }}
            >
              <div
                className="h-full rounded-full transition-all duration-75"
                style={{
                  width: `${progress}%`,
                  background: "linear-gradient(90deg, #7c3aed, #4f46e5, #06b6d4)",
                  boxShadow: "0 0 10px rgba(124,58,237,0.5)",
                }}
              />
            </div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <Sidebar collapsible="icon" className="border-r border-sidebar-border bg-sidebar backdrop-blur-xl">
      <SidebarHeader className="border-b border-sidebar-border px-4 py-3">
        <div className="flex items-center gap-3">
          <div className="flex size-9 items-center justify-center rounded-lg bg-primary/20 text-primary">
            <Car className="size-5" />
          </div>
          <div className={cn('flex flex-col transition-opacity', isCollapsed && 'opacity-0')}>
            <span className="text-sm font-semibold text-foreground">FleetCommand</span>
            <span className="text-xs text-muted-foreground">Operations Center</span>
          </div>
        </div>
      </SidebarHeader>

      <SidebarContent className="px-2 py-4">
        <SidebarGroup>
          <SidebarGroupContent>

            <SidebarMenu>
              {navItems.map((item) => {
                const isActive = pathname === item.href
                return (
                  <SidebarMenuItem key={item.href}>
                    <Tooltip>
                      <TooltipTrigger asChild>
                        <SidebarMenuButton
                          asChild
                          isActive={isActive}
                          className={cn(
                            'relative h-11 transition-all border border-transparent',
                            isActive ? 'bg-sidebar-accent/50 text-[#A78BFA] drop-shadow-[0_0_8px_rgba(167,139,250,0.3)] border-[#7C3AED]/25' : 'text-muted-foreground/60 hover:text-[#FFFFFF] hover:bg-sidebar-accent/10'
                          )}
                        >
                          <Link href={item.href}>
                            <item.icon
                              strokeWidth={1.5}
                              className={cn(
                                'size-5 transition-all',
                                isActive ? 'text-[#A78BFA] filter drop-shadow-[0_0_4px_rgba(167,139,250,0.6)]' : 'text-[#9CA3AF]/60'
                              )}
                            />
                            <span className={cn('flex-1 transition-all', isActive ? 'text-[#A78BFA] font-bold' : 'text-[#9CA3AF]')}>
                              {item.title}
                            </span>
                            {!isCollapsed && (
                              <Badge
                                variant="outline"
                                className={cn(
                                  'ml-auto text-[10px] h-5 px-1.5 font-medium border transition-all',
                                  isActive ? 'bg-[#7C3AED]/20 text-[#A78BFA] border-[#7C3AED]/40' : getBadgeStyles(item.badgeVariant)
                                )}
                              >
                                {item.badge}
                              </Badge>
                            )}
                            {isActive && (
                              <span className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-6 bg-[#7C3AED] rounded-r-full shadow-[0_0_10px_rgba(124,58,237,0.8)]" />
                            )}
                          </Link>
                        </SidebarMenuButton>
                      </TooltipTrigger>
                      {isCollapsed && (
                        <TooltipContent side="right" className="flex items-center gap-2">
                          {item.title}
                          <Badge
                            variant="outline"
                            className={cn('text-[10px] h-5 px-1.5', getBadgeStyles(item.badgeVariant))}
                          >
                            {item.badge}
                          </Badge>
                        </TooltipContent>
                      )}
                    </Tooltip>
                  </SidebarMenuItem>
                )
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter className="border-t border-sidebar-border p-2 flex flex-col gap-1">
        {/* Sign Out Button */}
        {!isCollapsed ? (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => {
              localStorage.removeItem("isLoggedIn")
              sessionStorage.removeItem("hasLoadedDashboard")
              window.location.href = "/login"
            }}
            className="w-full justify-start gap-2 text-destructive hover:text-destructive hover:bg-destructive/10"
          >
            <svg viewBox="0 0 24 24" fill="none" className="size-4 animate-pulse" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
              <polyline points="16 17 21 12 16 7" />
              <line x1="21" y1="12" x2="9" y2="12" />
            </svg>
            <span>Sign Out</span>
          </Button>
        ) : (
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => {
                  localStorage.removeItem("isLoggedIn")
                  sessionStorage.removeItem("hasLoadedDashboard")
                  window.location.href = "/login"
                }}
                className="w-full justify-center text-destructive hover:text-destructive hover:bg-destructive/10"
              >
                <svg viewBox="0 0 24 24" fill="none" className="size-4 animate-pulse" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
                  <polyline points="16 17 21 12 16 7" />
                  <line x1="21" y1="12" x2="9" y2="12" />
                </svg>
              </Button>
            </TooltipTrigger>
            <TooltipContent side="right">
              Sign Out
            </TooltipContent>
          </Tooltip>
        )}

        <Button
          variant="ghost"
          size="sm"
          onClick={toggleSidebar}
          className="w-full justify-start gap-2 text-muted-foreground hover:text-foreground"
        >
          <ChevronLeft
            className={cn(
              'size-4 transition-transform duration-200',
              isCollapsed && 'rotate-180'
            )}
          />
          {!isCollapsed && <span>Collapse Sidebar</span>}
        </Button>
      </SidebarFooter>
    </Sidebar>
  )
}
