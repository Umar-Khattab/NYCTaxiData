import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr'
import { useCallback, useEffect, useMemo, useState } from 'react'
import type {
  SimulationStartRequest,
  SimulationStatusResponse,
  SimulationTick,
  ZoneMetricPoint,
} from '../types/simulation'

const MAX_POINTS = 48

export function useSimulation() {
  const [connection, setConnection] = useState<HubConnection | null>(null)
  const [status, setStatus] = useState<SimulationStatusResponse | null>(null)
  const [latestTick, setLatestTick] = useState<SimulationTick | null>(null)
  const [aggregateSeries, setAggregateSeries] = useState<
    { time: string; demand: number; revenue: number }[]
  >([])
  const [zoneHistory, setZoneHistory] = useState<Record<number, ZoneMetricPoint[]>>({})
  const [availableZones, setAvailableZones] = useState<number[]>([])
  const apiBase =
    import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') || window.location.origin

  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl(`${apiBase}/hubs/simulation`)
      .withAutomaticReconnect()
      .build()

    conn.on('SimulationStatus', (payload: SimulationStatusResponse) => {
      setStatus(payload)
    })

    conn.on('SimulationTick', (tick: SimulationTick) => {
      setLatestTick(tick)
      setAggregateSeries((prev) => {
        const next = [
          ...prev,
          {
            time: new Date(tick.simulatedTime).toLocaleTimeString([], {
              hour: '2-digit',
              minute: '2-digit',
            }),
            demand: tick.aggregate.totalDemand,
            revenue: tick.aggregate.totalRevenue,
          },
        ]
        return next.slice(-MAX_POINTS)
      })

      setZoneHistory((prev) => {
        const updated: Record<number, ZoneMetricPoint[]> = { ...prev }
        for (const zone of tick.zones) {
          const history = updated[zone.zoneId] ? [...updated[zone.zoneId]] : []
          history.push({
            simulatedTime: tick.simulatedTime,
            demand: zone.demand,
            revenue: zone.revenue,
            etaMinutes: zone.etaMinutes,
            stockoutRisk: zone.stockoutRisk,
            driverCount: zone.driverCount,
            activeTrips: zone.activeTrips,
          })
          updated[zone.zoneId] = history.slice(-MAX_POINTS)
        }
        return updated
      })

      setAvailableZones(tick.zones.map((zone) => zone.zoneId))
    })

    conn
      .start()
      .then(async () => {
        setConnection(conn)
        try {
          const currentStatus = await conn.invoke<SimulationStatusResponse>('GetStatus')
          setStatus(currentStatus)
        } catch (error) {
          console.warn('Failed to fetch initial status', error)
        }
      })
      .catch((error) => console.error('SignalR connection failed', error))

    return () => {
      conn.stop()
    }
  }, [apiBase])

  const startSimulation = useCallback(
    async (request: SimulationStartRequest) => {
      if (!connection) return
      await connection.invoke('StartSimulation', request)
    },
    [connection],
  )

  const sendControl = useCallback(
    async (action: string, speedFactor?: number) => {
      if (!connection) return
      await connection.invoke('ControlSimulation', {
        action,
        speedFactor,
      })
    },
    [connection],
  )

  const isRunning = status?.status === 'Running'
  const isPaused = status?.isPaused ?? false

  return useMemo(
    () => ({
      status,
      latestTick,
      aggregateSeries,
      zoneHistory,
      availableZones,
      isRunning,
      isPaused,
      startSimulation,
      sendControl,
    }),
    [
      status,
      latestTick,
      aggregateSeries,
      zoneHistory,
      availableZones,
      isRunning,
      isPaused,
      startSimulation,
      sendControl,
    ],
  )
}
