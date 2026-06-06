'use client'

import * as React from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import {
  X,
  TrendingUp,
  TrendingDown,
  DollarSign,
  Users,
  Zap,
  AlertTriangle,
  MapPin,
  Clock,
  ArrowUpRight,
  Brain,
  Activity,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'

export interface InsightCardData {
  zoneId: string
  zoneName: string
  /** 0–100 demand score */
  demandScore: number
  /** 0–100 supply score */
  supplyScore: number
  trips: number
  avgFare: string
  /** gap = demand - supply */
  gap: number
  /** null = no dollar marker, 'high' = high profit */
  profitTier: 'high' | 'medium' | 'low' | null
  /** Whether this zone has a pulsing demand spike */
  hasDemandSpike: boolean
  /** AI explanation text */
  explanation: string
  /** Recommended action */
  recommendation: string
}

interface InsightCardProps {
  data: InsightCardData
  position: { x: number; y: number }
  containerSize: { width: number; height: number }
  onClose: () => void
  onDispatch?: (zoneId: string) => void
}

export function InsightCard({
  data,
  position,
  containerSize,
  onClose,
  onDispatch,
}: InsightCardProps) {
  const cardWidth = 320
  const cardHeight = 420

  // Smart position: flip left/right and up/down if near edges
  const adjustedX = position.x + cardWidth + 20 > containerSize.width
    ? position.x - cardWidth - 10
    : position.x + 16
  const adjustedY = position.y + cardHeight + 20 > containerSize.height
    ? position.y - cardHeight + 20
    : position.y

  const demandLevel =
    data.demandScore > 75 ? 'Peak' : data.demandScore > 50 ? 'High' : data.demandScore > 25 ? 'Moderate' : 'Low'

  const gapLabel = data.gap > 30 ? 'Surge' : data.gap > 0 ? 'Slight shortage' : data.gap < -30 ? 'Surplus' : 'Balanced'
  const gapColor =
    data.gap > 30
      ? 'text-red-400'
      : data.gap > 0
        ? 'text-amber-400'
        : 'text-cyan-400'

  return (
    <AnimatePresence>
      <motion.div
        key="insight-card"
        initial={{ opacity: 0, scale: 0.88, y: 8 }}
        animate={{ opacity: 1, scale: 1, y: 0 }}
        exit={{ opacity: 0, scale: 0.88, y: 8 }}
        transition={{ duration: 0.18, ease: [0.16, 1, 0.3, 1] }}
        className="absolute z-50 pointer-events-auto"
        style={{ left: adjustedX, top: adjustedY, width: cardWidth }}
      >
        {/* Card shell */}
        <div
          className="rounded-2xl border border-white/10 shadow-2xl overflow-hidden"
          style={{
            background: 'linear-gradient(145deg, rgba(10,8,22,0.97) 0%, rgba(14,12,30,0.97) 100%)',
            backdropFilter: 'blur(20px)',
            boxShadow:
              '0 0 0 1px rgba(139,92,246,0.2), 0 20px 60px rgba(0,0,0,0.7), 0 0 30px rgba(139,92,246,0.08)',
          }}
        >
          {/* Header */}
          <div className="flex items-start justify-between p-4 pb-3 border-b border-white/[0.07]">
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <MapPin className="size-3.5 text-violet-400 flex-shrink-0" />
                <span className="text-[9px] font-mono font-bold uppercase tracking-[0.15em] text-violet-400">
                  Zone Insight
                </span>
              </div>
              <h3 className="font-bold text-sm text-white truncate">{data.zoneName}</h3>
              <p className="text-[10px] text-slate-400 font-mono mt-0.5">
                {data.zoneId.slice(-8).toUpperCase()}
              </p>
            </div>
            <div className="flex items-center gap-1.5 ml-2 flex-shrink-0">
              {data.profitTier === 'high' && (
                <div
                  className="flex items-center justify-center size-7 rounded-lg text-yellow-300"
                  style={{
                    background: 'rgba(253,197,0,0.15)',
                    border: '1px solid rgba(253,197,0,0.35)',
                    boxShadow: '0 0 12px rgba(253,197,0,0.3)',
                  }}
                  title="High-profit zone"
                >
                  <DollarSign className="size-3.5" />
                </div>
              )}
              {data.hasDemandSpike && (
                <div
                  className="flex items-center justify-center size-7 rounded-lg text-red-400 animate-pulse"
                  style={{
                    background: 'rgba(248,113,113,0.12)',
                    border: '1px solid rgba(248,113,113,0.3)',
                  }}
                  title="Active demand spike"
                >
                  <Zap className="size-3.5" />
                </div>
              )}
              <Button
                variant="ghost"
                size="icon"
                className="size-7 text-slate-400 hover:text-white hover:bg-white/10"
                onClick={onClose}
              >
                <X className="size-3.5" />
              </Button>
            </div>
          </div>

          {/* Metrics grid */}
          <div className="grid grid-cols-3 gap-px p-4 bg-white/[0.02]">
            <MetricCell
              label="Demand"
              value={data.trips.toString()}
              sub={demandLevel}
              color={data.demandScore > 70 ? '#F87171' : data.demandScore > 40 ? '#FB923C' : '#A78BFA'}
            />
            <MetricCell
              label="Avg Fare"
              value={`$${data.avgFare}`}
              sub="per ride"
              color="#4ADE80"
            />
            <MetricCell
              label="Gap"
              value={data.gap > 0 ? `+${data.gap}` : data.gap.toString()}
              sub={gapLabel}
              color={data.gap > 30 ? '#F87171' : data.gap < -30 ? '#38BDF8' : '#A78BFA'}
            />
          </div>

          {/* Demand / Supply Bar */}
          <div className="px-4 pb-3 space-y-2">
            <MiniBar label="Demand" value={data.demandScore} color="#F87171" />
            <MiniBar label="Supply" value={data.supplyScore} color="#38BDF8" />
          </div>

          {/* AI Explanation */}
          <div
            className="mx-4 mb-3 p-3 rounded-xl text-[11px] leading-relaxed"
            style={{
              background: 'rgba(139,92,246,0.08)',
              border: '1px solid rgba(139,92,246,0.2)',
            }}
          >
            <div className="flex items-center gap-1.5 mb-1.5">
              <Brain className="size-3 text-violet-400 flex-shrink-0" />
              <span className="text-[9px] font-mono font-bold uppercase tracking-widest text-violet-400">
                AI Analysis
              </span>
            </div>
            <p className="text-slate-300">{data.explanation}</p>
          </div>

          {/* Recommendation */}
          <div
            className="mx-4 mb-3 p-3 rounded-xl text-[11px] leading-relaxed"
            style={{
              background: 'rgba(34,197,94,0.07)',
              border: '1px solid rgba(34,197,94,0.2)',
            }}
          >
            <div className="flex items-center gap-1.5 mb-1.5">
              <Activity className="size-3 text-emerald-400 flex-shrink-0" />
              <span className="text-[9px] font-mono font-bold uppercase tracking-widest text-emerald-400">
                Recommended Action
              </span>
            </div>
            <p className="text-slate-300">{data.recommendation}</p>
          </div>

          {/* CTA */}
          <div className="px-4 pb-4">
            <Button
              className="w-full h-9 text-xs font-bold gap-2"
              style={{
                background: 'linear-gradient(135deg, #7C3AED 0%, #4F46E5 100%)',
                boxShadow: '0 0 16px rgba(124,58,237,0.4)',
              }}
              onClick={() => onDispatch?.(data.zoneId)}
            >
              <ArrowUpRight className="size-3.5" />
              Dispatch Fleet to Zone
            </Button>
          </div>
        </div>
      </motion.div>
    </AnimatePresence>
  )
}

function MetricCell({
  label,
  value,
  sub,
  color,
}: {
  label: string
  value: string
  sub: string
  color: string
}) {
  return (
    <div className="text-center py-2">
      <p className="text-[9px] uppercase tracking-[0.12em] text-slate-500 font-bold mb-1">{label}</p>
      <p className="text-base font-black font-mono text-white" style={{ color }}>
        {value}
      </p>
      <p className="text-[9px] text-slate-500 mt-0.5">{sub}</p>
    </div>
  )
}

function MiniBar({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <div className="flex items-center gap-2">
      <span className="text-[9px] text-slate-500 uppercase tracking-wider font-bold w-12 flex-shrink-0">
        {label}
      </span>
      <div className="flex-1 h-1.5 bg-white/5 rounded-full overflow-hidden">
        <div
          className="h-full rounded-full transition-all duration-700"
          style={{
            width: `${Math.min(100, Math.max(0, value))}%`,
            background: color,
            boxShadow: `0 0 6px ${color}60`,
          }}
        />
      </div>
      <span className="text-[9px] font-mono text-slate-400 w-8 text-right">{value}%</span>
    </div>
  )
}
