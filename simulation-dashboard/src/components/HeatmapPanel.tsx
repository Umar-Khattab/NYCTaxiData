import { useEffect, useState, useRef } from 'react'
import type { ZoneSimulationSnapshot } from '../types/simulation'

interface HeatmapPanelProps {
  zones: ZoneSimulationSnapshot[]
}

interface ZoneDto {
  zoneId: number
  zoneName: string
  centerLatitude: number
  centerLongitude: number
  osmId?: number
}

export function HeatmapPanel({ zones }: HeatmapPanelProps) {
  const [coordsMap, setCoordsMap] = useState<Record<number, ZoneDto> | null>(null)
  const [isLeafletReady, setIsLeafletReady] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const mapContainerRef = useRef<HTMLDivElement>(null)
  const mapRef = useRef<any>(null)
  const circlesRef = useRef<Record<number, any>>({})

  const apiBase =
    import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') || window.location.origin

  // 1. Fetch zones metadata/coordinates lookup map
  useEffect(() => {
    fetch(`${apiBase}/api/v1/zones`)
      .then((res) => {
        if (!res.ok) throw new Error('Failed to load zones coordinates')
        return res.json() as Promise<ZoneDto[]>
      })
      .then((data) => {
        const mapping: Record<number, ZoneDto> = {}
        data.forEach((z) => {
          mapping[z.zoneId] = z
        })
        setCoordsMap(mapping)
      })
      .catch((err) => {
        console.error('Error fetching zone coordinates', err)
        setError('Unable to load zone spatial data from backend.')
      })
  }, [apiBase])

  // 2. Poll/Check if Leaflet script is loaded on window
  useEffect(() => {
    const checkL = () => {
      if ((window as any).L) {
        setIsLeafletReady(true)
      } else {
        setTimeout(checkL, 100)
      }
    }
    checkL()
  }, [])

  // 3. Initialize Leaflet Map
  useEffect(() => {
    if (!isLeafletReady || !mapContainerRef.current || mapRef.current) return

    const L = (window as any).L
    // Center map around Mid-town Manhattan / NYC center
    const map = L.map(mapContainerRef.current).setView([40.7306, -73.9352], 11)

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
    }).addTo(map)

    mapRef.current = map

    return () => {
      if (mapRef.current) {
        mapRef.current.remove()
        mapRef.current = null
        circlesRef.current = {}
      }
    }
  }, [isLeafletReady])

  // 4. Render circles & updates dynamically on simulation ticks
  useEffect(() => {
    if (!mapRef.current || !coordsMap || !zones || zones.length === 0) return

    const L = (window as any).L
    const maxDemand = Math.max(1, ...zones.map((zone) => zone.demand))

    // Remove old circles for zones that are no longer in the active zones list
    const activeZoneIds = new Set(zones.map((z) => z.zoneId))
    Object.keys(circlesRef.current).forEach((key) => {
      const id = parseInt(key, 10)
      if (!activeZoneIds.has(id)) {
        circlesRef.current[id].remove()
        delete circlesRef.current[id]
      }
    })

    // Create or update circle markers
    zones.forEach((zone) => {
      const coord = coordsMap[zone.zoneId]
      if (coord && coord.centerLatitude && coord.centerLongitude) {
        const intensity = zone.demand / maxDemand
        
        // Custom radius (in meters) scaled with demand intensity
        const radius = 150 + intensity * 450
        
        // Color based on demand level
        const color = intensity > 0.8 ? '#dc2626' : intensity > 0.5 ? '#f97316' : '#2563eb' // Red, Orange, Blue
        const fillOpacity = 0.35 + intensity * 0.45

        const popupContent = `
          <div style="font-family: sans-serif; font-size: 13px; line-height: 1.4; color: #334155;">
            <strong style="font-size: 14px; color: #0f172a;">${coord.zoneName}</strong><br/>
            <span style="color: #64748b;">Zone ID: ${zone.zoneId}</span><br/>
            <hr style="margin: 6px 0; border: 0; border-top: 1px solid #e2e8f0;" />
            <strong>Demand:</strong> ${zone.demand.toFixed(1)}<br/>
            <strong>Active Drivers:</strong> ${zone.driverCount}<br/>
            <strong>Active Trips:</strong> ${zone.activeTrips}<br/>
            <strong>Revenue:</strong> $${zone.revenue.toFixed(2)}<br/>
            <strong>Stockout Risk:</strong> ${(zone.stockoutRisk * 100).toFixed(1)}%
          </div>
        `

        if (circlesRef.current[zone.zoneId]) {
          const circle = circlesRef.current[zone.zoneId]
          circle.setRadius(radius)
          circle.setStyle({
            color: color,
            fillColor: color,
            fillOpacity: fillOpacity,
          })
          circle.setPopupContent(popupContent)
        } else {
          const circle = L.circle([coord.centerLatitude, coord.centerLongitude], {
            color: color,
            fillColor: color,
            fillOpacity: fillOpacity,
            weight: 1.5,
            radius: radius,
          }).addTo(mapRef.current)

          circle.bindPopup(popupContent)
          circlesRef.current[zone.zoneId] = circle
        }
      }
    })
  }, [coordsMap, zones])

  return (
    <div className="card heatmap-panel" style={{ minWidth: '320px', gridColumn: 'span 2' }}>
      <header>
        <h3>Live Geographic Demand Map</h3>
        <span>Real-time spatial demand intensity using database coordinates</span>
      </header>

      {error && <div style={{ color: '#ef4444', fontSize: '13px' }}>{error}</div>}

      <div
        ref={mapContainerRef}
        id="leaflet-heatmap-map"
        style={{
          height: '380px',
          width: '100%',
          borderRadius: '12px',
          border: '1px solid #e2e8f0',
          backgroundColor: '#f1f5f9',
          zIndex: 1,
        }}
      >
        {!isLeafletReady && (
          <div
            style={{
              display: 'flex',
              height: '100%',
              alignItems: 'center',
              justifyContent: 'center',
              color: '#64748b',
              fontSize: '14px',
            }}
          >
            Loading Leaflet Map Engine...
          </div>
        )}
      </div>
    </div>
  )
}
