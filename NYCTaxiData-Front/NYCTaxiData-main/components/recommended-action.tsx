'use client'

import * as React from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Download, FileJson, FileText, AlertCircle, Lightbulb } from 'lucide-react'
import type { SimulationResult } from '@/stores/use-simulation-engine-store'

interface RecommendedActionProps {
  result: SimulationResult | null
  isLoading: boolean
  onExport?: (format: 'csv' | 'pdf' | 'json') => void
}

export function RecommendedAction({ result, isLoading, onExport }: RecommendedActionProps) {
  const handleExport = (format: 'csv' | 'pdf' | 'json') => {
    if (onExport) {
      onExport(format)
    } else {
      // Default export implementation
      if (result) {
        const dataStr = JSON.stringify(result, null, 2)
        const dataBlob = new Blob([dataStr], { type: 'application/json' })
        const url = URL.createObjectURL(dataBlob)
        const link = document.createElement('a')
        link.href = url
        link.download = `simulation-result-${new Date().toISOString().slice(0, 10)}.${format}`
        link.click()
        URL.revokeObjectURL(url)
      }
    }
  }

  return (
    <div className="rounded-3xl border border-white/10 bg-slate-950/40 p-5 shadow-[0_0_40px_rgba(124,58,237,0.12)] backdrop-blur-2xl h-full flex flex-col">
      <div className="mb-4">
        <p className="text-sm text-muted-foreground">Decision Support</p>
        <h2 className="text-xl font-semibold text-foreground">Recommended Action</h2>
      </div>

      {isLoading ? (
        <div className="flex-1 flex items-center justify-center">
          <div className="text-center">
            <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-primary" />
            <p className="mt-2 text-xs text-muted-foreground">Generating recommendation...</p>
          </div>
        </div>
      ) : result ? (
        <div className="flex-1 space-y-4 overflow-y-auto">
          {/* Main Recommendation Card */}
          <Card className="rounded-2xl border-l-4 border-l-emerald-500 bg-emerald-500/10 border-emerald-500/30 p-4">
            <div className="flex gap-3">
              <Lightbulb className="size-5 flex-shrink-0 text-emerald-300 mt-1" />
              <div>
                <p className="font-semibold text-foreground text-sm">{result.recommendation}</p>
                <p className="mt-2 text-xs text-muted-foreground leading-relaxed">
                  This strategy optimizes fleet positioning to maximize demand fulfillment while managing operational costs.
                </p>
              </div>
            </div>
          </Card>

          {/* Impact Metrics */}
          <div className="grid gap-2">
            <div className="rounded-2xl border border-white/10 bg-white/5 p-3">
              <div className="flex items-center justify-between mb-2">
                <p className="text-xs font-semibold text-muted-foreground uppercase tracking-[0.1em]">
                  P50 Profit Impact
                </p>
                <Badge className="bg-cyan-500/20 text-cyan-300 border-cyan-500/30">
                  +${result.p50_impact}K
                </Badge>
              </div>
              <div className="w-full bg-slate-900 rounded-full h-1.5 overflow-hidden">
                <div
                  className="bg-gradient-to-r from-cyan-500 to-emerald-500 h-full"
                  style={{ width: `${Math.min(100, (result.p50_impact / 10000) * 100)}%` }}
                />
              </div>
            </div>

            <div className="rounded-2xl border border-white/10 bg-white/5 p-3">
              <div className="flex items-center justify-between mb-2">
                <p className="text-xs font-semibold text-muted-foreground uppercase tracking-[0.1em]">
                  P90 Profit Impact
                </p>
                <Badge className="bg-amber-500/20 text-amber-300 border-amber-500/30">
                  +${result.p90_impact}K
                </Badge>
              </div>
              <div className="w-full bg-slate-900 rounded-full h-1.5 overflow-hidden">
                <div
                  className="bg-gradient-to-r from-amber-500 to-orange-500 h-full"
                  style={{ width: `${Math.min(100, (result.p90_impact / 10000) * 100)}%` }}
                />
              </div>
            </div>
          </div>

          {/* Export Section */}
          <div className="space-y-2 border-t border-white/10 pt-4 mt-4">
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-[0.1em]">
              Export Plan
            </p>
            <div className="grid grid-cols-3 gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleExport('json')}
                className="text-xs h-8 gap-1"
              >
                <FileJson className="size-3" />
                JSON
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleExport('csv')}
                className="text-xs h-8 gap-1"
              >
                <FileText className="size-3" />
                CSV
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handleExport('pdf')}
                className="text-xs h-8 gap-1"
              >
                <Download className="size-3" />
                PDF
              </Button>
            </div>
          </div>

          {/* Disclaimer */}
          <div className="rounded-2xl border border-amber-500/30 bg-amber-500/10 p-3">
            <div className="flex gap-2">
              <AlertCircle className="size-4 flex-shrink-0 text-amber-300 mt-0.5" />
              <p className="text-xs text-amber-200 leading-relaxed">
                Recommendations are based on historical patterns and current market conditions. Verify with operations team before deployment.
              </p>
            </div>
          </div>
        </div>
      ) : (
        <div className="flex-1 flex items-center justify-center">
          <p className="text-xs text-muted-foreground text-center">Run simulation to generate recommendations</p>
        </div>
      )}
    </div>
  )
}
