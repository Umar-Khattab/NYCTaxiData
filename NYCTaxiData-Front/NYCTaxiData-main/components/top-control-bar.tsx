'use client'

import * as React from 'react'
import {
  Activity,
  Car,
  DollarSign,
  TrendingUp,
  Clock,
  Calendar,
  ChevronDown,
  Layers,
  Sun,
  Moon
} from 'lucide-react'

import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { CountingUp } from '@/components/ui/counting-up'
import { useTheme } from 'next-themes'

// 1. استدعاء هوك جلب الـ KPIs الحقيقي (عدلي المسار حسب مكان الـ hooks عندك)
import { useAnalyticsKPIs } from '@/hooks/use' 

interface KPICardProps {
  title: string
  numericValue: number
  prefix?: string
  suffix?: string
  decimals?: number
  change?: string
  changeType?: 'positive' | 'negative' | 'neutral'
  icon: React.ElementType
}

function KPICard({ 
  title, 
  numericValue, 
  prefix = '', 
  suffix = '', 
  decimals = 0, 
  change, 
  changeType = 'neutral', 
  icon: Icon 
}: KPICardProps) {
  let badgeStyle = "text-muted-foreground bg-secondary/50 border-border"
  if (title === "Active Taxis" || changeType === "positive") {
    badgeStyle = "text-success bg-success/10 border-success/20 glow-green"
  } else if (title === "Demand Index") {
    badgeStyle = "text-warning bg-warning/10 border-warning/20 glow-amber"
  } else if (title === "Idle Rate") {
    badgeStyle = "text-accent bg-accent/10 border-accent/20 glow-lavender"
  } else if (title === "Average Fare") {
    badgeStyle = "text-success bg-success/10 border-success/20 glow-green"
  }

  return (
    <div className="relative overflow-hidden rounded-xl border border-border bg-card p-4 flex items-center justify-between transition-all hover:border-primary/30 hover:shadow-[0_0_15px_rgba(124,58,237,0.06)] dark:hover:shadow-[0_0_15px_rgba(124,58,237,0.15)] theme-transition select-none">
      <div className="absolute inset-0 bg-gradient-to-r from-primary/[0.02] to-transparent pointer-events-none" />
      <div className="flex items-center gap-3.5 z-10">
        <div className="flex size-11 items-center justify-center rounded-xl bg-primary/10 border border-primary/20">
          <Icon className="size-5 text-primary filter drop-shadow-[0_0_4px_rgba(167,139,250,0.3)]" />
        </div>
        <div className="flex flex-col">
          <span className="text-[11px] text-muted-foreground uppercase tracking-wider font-semibold">{title}</span>
          <span className="text-2xl font-black text-foreground leading-tight mt-1 tracking-tight">
            <CountingUp 
              value={numericValue} 
              prefix={prefix} 
              suffix={suffix} 
              decimals={decimals} 
            />
          </span>
        </div>
      </div>
      {change && (
        <span className={cn(
          "text-[10px] font-bold px-2 py-0.5 rounded border font-mono z-10",
          badgeStyle
        )}>
          {change}
        </span>
      )}
    </div>
  )
}

const timeRanges = [
  { label: 'Last Hour', value: '1h' },
  { label: 'Last 6 Hours', value: '6h' },
  { label: 'Last 24 Hours', value: '24h' },
  { label: 'Last 7 Days', value: '7d' },
  { label: 'Last 30 Days', value: '30d' },
]

export interface TopControlBarProps {
  viewMode?: string
  onViewModeChange?: (mode: string) => void
  showDemand?: boolean
  onShowDemandChange?: (show: boolean) => void
  showFleet?: boolean
  onShowFleetChange?: (show: boolean) => void
  showGap?: boolean
  onShowGapChange?: (show: boolean) => void
}

export function TopControlBar({ 
  viewMode = 'realtime', 
  onViewModeChange,
  showDemand = true,
  onShowDemandChange,
  showFleet = false,
  onShowFleetChange,
  showGap = false,
  onShowGapChange
}: TopControlBarProps) {
  const [mounted, setMounted] = React.useState(false)
  const { resolvedTheme, setTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'

  React.useEffect(() => {
    setMounted(true)
  }, [])

  const toggleTheme = () => {
    setTheme(isDark ? 'light' : 'dark')
  }

  const [timeRange, setTimeRange] = React.useState('24h')
  const selectedTimeRange = timeRanges.find((t) => t.value === timeRange)

  // 2. تشغيل الهوك لجلب البيانات الحية وحالة التحميل والخطأ
  const { data: kpiData, isLoading, isError } = useAnalyticsKPIs()

  return (
    <div className="flex flex-col gap-4 p-4 border-b border-border bg-card/40 backdrop-blur-md theme-transition">
      {/* Top row: Toggle buttons and theme selectors */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        
        {/* Cybersecurity style hardware selection bar */}
        <div className="flex flex-wrap items-center gap-1.5 bg-secondary p-1.5 rounded-xl border border-border shadow-inner">
          <button
            onClick={() => onViewModeChange?.('realtime')}
            className={cn(
              'flex items-center gap-2 text-xs px-4 py-2 rounded-lg font-medium transition-all select-none border border-transparent',
              viewMode === 'realtime' 
                ? 'bg-primary/20 border-primary/30 text-primary glow-purple font-bold'
                : 'text-muted-foreground hover:text-foreground hover:bg-muted'
            )}
          >
            <Activity className="size-3.5" />
            Demand Grid
          </button>
          
          <button
            onClick={() => onViewModeChange?.('predictive')}
            className={cn(
              'flex items-center gap-2 text-xs px-4 py-2 rounded-lg font-medium transition-all select-none border border-transparent',
              viewMode === 'predictive'
                ? 'bg-primary/20 border-primary/30 text-primary glow-purple font-bold'
                : 'text-muted-foreground hover:text-foreground hover:bg-muted'
            )}
          >
            <TrendingUp className="size-3.5" />
            Predictive Model (6h)
          </button>
          
          <div className="hidden sm:block w-[1px] h-5 bg-border mx-1" />
          
          {/* Overlay toggles */}
          <button
            onClick={() => onShowDemandChange?.(!showDemand)}
            className={cn(
              'flex items-center gap-2 text-xs px-3.5 py-2 rounded-lg font-medium transition-all select-none border border-transparent',
              showDemand 
                ? 'bg-primary/10 border-primary/20 text-foreground font-semibold shadow-sm'
                : 'text-muted-foreground hover:text-foreground hover:bg-muted'
            )}
          >
            <Layers className="size-3.5 text-accent" />
            Heatmap Layer
          </button>

          <button
            onClick={() => onShowFleetChange?.(!showFleet)}
            className={cn(
              'flex items-center gap-2 text-xs px-3.5 py-2 rounded-lg font-medium transition-all select-none border border-transparent',
              showFleet
                ? 'bg-primary/10 border-primary/20 text-foreground font-semibold shadow-sm'
                : 'text-muted-foreground hover:text-foreground hover:bg-muted'
            )}
          >
            <Car className="size-3.5 text-[#06B6D4]" />
            Live Taxis
          </button>

          <button
            onClick={() => onShowGapChange?.(!showGap)}
            className={cn(
              'flex items-center gap-2 text-xs px-3.5 py-2 rounded-lg font-medium transition-all select-none border border-transparent',
              showGap
                ? 'bg-[#EF4444]/10 border-[#EF4444]/20 text-foreground font-semibold shadow-sm'
                : 'text-muted-foreground hover:text-foreground hover:bg-muted'
            )}
          >
            <TrendingUp className="size-3.5 text-[#EF4444]" />
            Supply-Demand Gap
          </button>
        </div>

        <div className="flex items-center justify-between md:justify-end gap-4">
          {/* Theme Toggle */}
          <button 
            onClick={toggleTheme}
            className="relative flex items-center justify-between bg-secondary border border-border h-9 w-24 rounded-full p-1 cursor-pointer theme-transition hover:border-primary/40 shadow-inner select-none"
            title={mounted && isDark ? "Switch to Clean Light" : "Switch to Ultra-Dark"}
          >
            <div className={cn(
              "absolute top-1 bottom-1 w-10 rounded-full bg-primary flex items-center justify-center text-primary-foreground shadow-md theme-transition",
              mounted && isDark ? "left-1" : "left-12"
            )}>
              {mounted && isDark ? <Moon className="size-3.5" /> : <Sun className="size-3.5 animate-spin-slow" />}
            </div>
            <span className="w-1/2 text-center text-[8px] font-black opacity-30 select-none pointer-events-none">DARK</span>
            <span className="w-1/2 text-center text-[8px] font-black opacity-30 select-none pointer-events-none">LIGHT</span>
          </button>

          {/* Time Range Selector */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" className="gap-2 bg-secondary/80 border-border hover:bg-secondary text-foreground text-xs h-9 theme-transition">
                <Calendar className="size-3.5 text-muted-foreground" />
                {selectedTimeRange?.label}
                <ChevronDown className="size-3.5 text-muted-foreground" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-40 bg-card border-border text-foreground theme-transition">
              {timeRanges.map((range) => (
                <DropdownMenuItem
                  key={range.value}
                  onClick={() => setTimeRange(range.value)}
                  className={cn(range.value === timeRange ? 'bg-primary/20 text-primary' : 'hover:bg-muted/40')}
                >
                  {range.label}
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>

      {/* Bottom row: الـ KPI Cards الحية بعد الربط */}
      {/* في حالة وجود خطأ أو تحميل بنعرض شكل الهيكل الاحتياطي الـ Skeleton أو الرسالة */}
      {isError ? (
        <div className="text-center text-xs text-red-500 py-2 border border-red-500/20 rounded-xl bg-red-500/5">
          فشل في تحديث مؤشرات الأداء الحية من السيرفر.
        </div>
      ) : (
        <div className={cn("grid grid-cols-2 lg:grid-cols-4 gap-4 transition-opacity", isLoading && "opacity-50")}>
          <KPICard
            title="Active Taxis"
            // لو البيانات لسه بتحمل هيحط الـ value القديم كـ Fallback أو 0
            numericValue={kpiData?.activeTaxis ?? 1247}
            change={kpiData?.activeTaxisChange ?? "+12%"}
            changeType={kpiData?.activeTaxisChangeType ?? "positive"}
            icon={Car}
          />
          <KPICard
            title="Average Fare"
            numericValue={kpiData?.averageFare ?? 28.50}
            prefix="$"
            decimals={2}
            change={kpiData?.averageFareChange ?? "+5.2%"}
            changeType={kpiData?.averageFareChangeType ?? "positive"}
            icon={DollarSign}
          />
          <KPICard
            title="Demand Index"
            numericValue={kpiData?.demandIndex ?? 8.7}
            decimals={1}
            change={kpiData?.demandIndexChange ?? "+2.1"}
            changeType={kpiData?.demandIndexChangeType ?? "positive"}
            icon={TrendingUp}
          />
          <KPICard
            title="Idle Rate"
            numericValue={kpiData?.idleRate ?? 14.2}
            suffix="%"
            decimals={1}
            change={kpiData?.idleRateChange ?? "-3.1%"}
            changeType={kpiData?.idleRateChangeType ?? "positive"}
            icon={Clock}
          />
        </div>
      )}
    </div>
  )
}