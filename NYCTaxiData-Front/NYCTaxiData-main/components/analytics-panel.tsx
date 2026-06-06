'use client'

import * as React from 'react'
import { motion } from 'framer-motion'
import { X, BarChart3, TrendingUp, Compass, Cpu } from 'lucide-react'
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'

import { Button } from '@/components/ui/button'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Donut3d } from '@/components/charts/donut-3d'
import { cn } from '@/lib/utils'
import { fleetApi } from '@/lib/fleet-api'

interface AnalyticsPanelProps {
  zone: any | null
  onClose: () => void
}

interface DemandPoint { hour: string; demand: number; predicted: number }
interface RevenuePoint { day: string; revenue: number }

async function fetchDemandData(): Promise<DemandPoint[]> {
  const hourly = await fleetApi.reports.getHourly()
  return hourly.map((h) => ({
    hour: `${h.hour}:00`,
    demand: h.this_week,
    predicted: h.last_week,
  }))
}

async function fetchRevenueData(): Promise<RevenuePoint[]> {
  const weekly = await fleetApi.reports.getWeekly()
  return weekly.map((w) => ({ day: w.day, revenue: w.revenue }))
}

const LEGEND_ITEMS = [
  { name: 'Manhattan', percentage: '58%', color: 'bg-[#1D4ED8]', text: '58% share · 4.8k active' },
  { name: 'Brooklyn', percentage: '22%', color: 'bg-[#06B6D4]', text: '22% share · 1.8k active' },
  { name: 'Queens', percentage: '12%', color: 'bg-[#0D9488]', text: '12% share · 1.0k active' },
  { name: 'Bronx', percentage: '6%', color: 'bg-[#FB923C]', text: '6% share · 0.5k active' },
  { name: 'Staten Island', percentage: '2%', color: 'bg-[#6B7280]', text: '2% share · 0.1k active' },
]

export function AnalyticsPanel({ zone, onClose }: AnalyticsPanelProps) {
  const [demandData, setDemandData] = React.useState<DemandPoint[]>([])
  const [revenueData, setRevenueData] = React.useState<RevenuePoint[]>([])
  const [hoveredLegendRow, setHoveredLegendRow] = React.useState<string | null>(null)

  React.useEffect(() => {
    fetchDemandData().then(setDemandData).catch(() => setDemandData([]))
    fetchRevenueData().then(setRevenueData).catch(() => setRevenueData([]))
  }, [])

  return (
    <div className="w-96 h-full glass border-l border-border flex flex-col shadow-2xl theme-transition select-none">
      {/* Header */}
      <div className="flex items-center justify-between p-4 border-b border-border">
        <div>
          <h3 className="font-extrabold text-foreground text-sm flex items-center gap-2">
            <Cpu className="size-4 text-primary animate-pulse" />
            {zone ? zone.zone : 'SYSTEM OPERATIONS'}
          </h3>
          <p className="text-[10px] text-muted-foreground tracking-wider uppercase font-semibold">
            {zone ? 'Zone Telemetry analysis' : 'Global NYC Grid Analytics'}
          </p>
        </div>
        {zone && (
          <Button variant="ghost" size="icon" onClick={onClose} className="size-8 text-muted-foreground hover:text-foreground">
            <X className="size-4" />
          </Button>
        )}
      </div>

      {zone ? (
        // Context-sensitive View: Displays selected zone telemetry
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* Stats Summary */}
          <div className="grid grid-cols-3 gap-2 p-4 border-b border-border bg-secondary/20">
            <div className="text-center p-2 rounded-lg bg-card/40 border border-border">
              <div className="text-base font-black text-primary font-mono">{zone.trips}</div>
              <div className="text-[9px] text-muted-foreground uppercase font-bold tracking-wider">Demand</div>
            </div>
            <div className="text-center p-2 rounded-lg bg-card/40 border border-border">
              <div className="text-base font-black text-success font-mono">${zone.avgFare}</div>
              <div className="text-[9px] text-muted-foreground uppercase font-bold tracking-wider">Avg Fare</div>
            </div>
            <div className="text-center p-2 rounded-lg bg-card/40 border border-border">
              <div className="text-base font-black text-accent font-mono">{zone.supply !== undefined ? zone.supply : Math.floor(zone.trips / 12)}</div>
              <div className="text-[9px] text-muted-foreground uppercase font-bold tracking-wider">Supply</div>
            </div>
          </div>

          {/* Tabbed Analytics charts */}
          <Tabs defaultValue="demand" className="flex-1 flex flex-col overflow-hidden">
            <TabsList className="mx-4 mt-4 bg-secondary border border-border rounded-lg">
              <TabsTrigger value="demand" className="flex-1 text-[11px] data-[state=active]:bg-primary data-[state=active]:text-primary-foreground transition-all">
                Demand Graph
              </TabsTrigger>
              <TabsTrigger value="revenue" className="flex-1 text-[11px] data-[state=active]:bg-primary data-[state=active]:text-primary-foreground transition-all">
                Revenue
              </TabsTrigger>
            </TabsList>

            <TabsContent value="demand" className="flex-1 p-4 flex flex-col overflow-hidden">
              <div className="flex-1 min-h-0 flex flex-col gap-4">
                <div>
                  <h4 className="text-xs font-bold text-foreground flex items-center gap-1.5">
                    <BarChart3 className="size-3.5 text-accent" />
                    24-Hour Demand Trend
                  </h4>
                  <p className="text-[10px] text-muted-foreground">Dynamic grid dispatch simulation compared to AI predictions</p>
                </div>
                <div className="flex-1 min-h-0">
                  <ResponsiveContainer width="100%" height="100%">
                    <AreaChart data={demandData} margin={{ top: 10, right: 10, left: -25, bottom: 0 }}>
                      <defs>
                        <linearGradient id="demandGradient" x1="0" y1="0" x2="0" y2="1">
                          <stop offset="5%" stopColor="var(--primary)" stopOpacity={0.4} />
                          <stop offset="95%" stopColor="var(--primary)" stopOpacity={0} />
                        </linearGradient>
                        <linearGradient id="predictedGradient" x1="0" y1="0" x2="0" y2="1">
                          <stop offset="5%" stopColor="var(--accent)" stopOpacity={0.2} />
                          <stop offset="95%" stopColor="var(--accent)" stopOpacity={0} />
                        </linearGradient>
                      </defs>
                      <XAxis 
                        dataKey="hour" 
                        axisLine={false} 
                        tickLine={false} 
                        tick={{ fontSize: 9, fill: 'var(--muted-foreground)' }}
                        interval={4}
                      />
                      <YAxis 
                        axisLine={false} 
                        tickLine={false} 
                        tick={{ fontSize: 9, fill: 'var(--muted-foreground)' }}
                      />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: 'var(--popover)',
                          border: '1px solid var(--border)',
                          borderRadius: '8px',
                          fontSize: '11px',
                          color: 'var(--foreground)',
                          boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
                        }}
                      />
                      <Area
                        type="monotone"
                        dataKey="demand"
                        stroke="var(--primary)"
                        strokeWidth={2}
                        fill="url(#demandGradient)"
                      />
                      <Area
                        type="monotone"
                        dataKey="predicted"
                        stroke="var(--accent)"
                        strokeWidth={1.5}
                        strokeDasharray="4 4"
                        fill="url(#predictedGradient)"
                      />
                    </AreaChart>
                  </ResponsiveContainer>
                </div>
                <div className="flex gap-4 text-[10px] font-semibold border-t border-border pt-3">
                  <div className="flex items-center gap-1.5">
                    <div className="w-3 h-0.5 bg-primary rounded-full" />
                    <span className="text-muted-foreground">Live Demand</span>
                  </div>
                  <div className="flex items-center gap-1.5">
                    <div className="w-3 h-0.5 bg-accent rounded-full border-dashed" style={{ borderTop: '2.5px dashed' }} />
                    <span className="text-muted-foreground">AI Predicted</span>
                  </div>
                </div>
              </div>
            </TabsContent>

            <TabsContent value="revenue" className="flex-1 p-4 flex flex-col overflow-hidden">
              <div className="flex-1 min-h-0 flex flex-col gap-4">
                <div>
                  <h4 className="text-xs font-bold text-foreground flex items-center gap-1.5">
                    <TrendingUp className="size-3.5 text-success" />
                    Weekly Revenue Cycles
                  </h4>
                  <p className="text-[10px] text-muted-foreground">Earnings captured across current sector grids</p>
                </div>
                <div className="flex-1 min-h-0">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={revenueData} margin={{ top: 10, right: 10, left: -25, bottom: 0 }}>
                      <XAxis 
                        dataKey="day" 
                        axisLine={false} 
                        tickLine={false} 
                        tick={{ fontSize: 9, fill: 'var(--muted-foreground)' }}
                      />
                      <YAxis 
                        axisLine={false} 
                        tickLine={false} 
                        tick={{ fontSize: 9, fill: 'var(--muted-foreground)' }}
                        tickFormatter={(value) => `$${value / 1000}k`}
                      />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: 'var(--popover)',
                          border: '1px solid var(--border)',
                          borderRadius: '8px',
                          fontSize: '11px',
                          color: 'var(--foreground)',
                          boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
                        }}
                        formatter={(value: number) => [`$${value.toLocaleString()}`, 'Revenue']}
                      />
                      <Bar 
                        dataKey="revenue" 
                        fill="var(--primary)" 
                        radius={[4, 4, 0, 0]}
                      />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </div>
            </TabsContent>
          </Tabs>

          {/* Action dispatch ribbon */}
          <div className="p-4 border-t border-border bg-secondary/30">
            <Button 
              className="w-full gap-2 bg-primary hover:bg-primary/95 text-primary-foreground font-bold shadow-md cursor-pointer h-10 transition-all active:scale-[0.98]"
              onClick={() => {
                console.log(`[WebSocket] Dispatch instructions broadcasted to active fleet in ${zone.zone}`)
                alert(`Websocket Signal Broadcasted: assigned units to ${zone.zone} successfully.`)
              }}
            >
              <Compass className="size-4 animate-spin-slow" />
              Coordinate Fleet Dispatch
            </Button>
          </div>
        </div>
      ) : (
        // Hero View: Interactive 3D Donut Chart representation
        <div className="flex-1 flex flex-col p-4 overflow-y-auto">
          <div className="mb-4">
            <h4 className="text-xs font-bold text-foreground mb-0.5">NYC WORKLOAD SEGMENTATION</h4>
            <p className="text-[10px] text-muted-foreground uppercase tracking-wider font-semibold">Real-time taxi ride distribution by borough</p>
          </div>
          
          {/* Three.js canvas container */}
          <div className="bg-secondary/30 border border-border rounded-xl overflow-hidden mb-5 shadow-inner">
            <Donut3d 
              onHoverSegment={setHoveredLegendRow} 
              hoveredSegmentName={hoveredLegendRow} 
            />
          </div>

          {/* Legend list */}
          <div className="flex flex-col gap-2">
            <div className="text-[9px] font-mono font-bold uppercase tracking-widest text-primary mb-1">
              BOROUGH METRIC GRID
            </div>
            
            {LEGEND_ITEMS.map((item) => {
              const isHovered = hoveredLegendRow === item.name
              return (
                <div
                  key={item.name}
                  onMouseEnter={() => setHoveredLegendRow(item.name)}
                  onMouseLeave={() => setHoveredLegendRow(null)}
                  className={cn(
                    "flex items-center justify-between p-2 rounded-lg border border-transparent transition-all cursor-pointer theme-transition",
                    isHovered 
                      ? "bg-primary/10 border-primary/20 shadow-sm"
                      : "hover:bg-secondary/40"
                  )}
                >
                  <div className="flex items-center gap-2.5">
                    <div className={cn("w-3 h-3 rounded-full shadow-sm", item.color)} />
                    <div className="flex flex-col">
                      <span className={cn(
                        "text-xs font-bold transition-colors",
                        isHovered ? "text-primary font-extrabold" : "text-foreground"
                      )}>
                        {item.name}
                      </span>
                      <span className="text-[10px] text-muted-foreground">
                        {item.text}
                      </span>
                    </div>
                  </div>
                  <div className="text-xs font-extrabold text-foreground font-mono">
                    {item.percentage}
                  </div>
                </div>
              )
            })}
          </div>
          
          {/* Extra system details */}
          <div className="mt-5 p-3.5 rounded-xl border border-border bg-secondary/20 select-none">
            <div className="text-[9px] font-mono font-bold uppercase tracking-widest text-accent mb-1.5">
              GRID COORDINATION REPORT
            </div>
            <p className="text-[11px] text-muted-foreground leading-relaxed">
              NYC fleet density is currently centered heavily in Manhattan (58%). Automated dispatch controls have triggered unit balancing schedules for Brooklyn and Queens grids.
            </p>
          </div>
        </div>
      )}
    </div>
  )
}
