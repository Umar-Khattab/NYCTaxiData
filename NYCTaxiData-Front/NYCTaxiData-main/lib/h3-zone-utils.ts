import type { MapViewState } from '@deck.gl/core'
import * as h3 from 'h3-js'
import { NYC_H3_RESOLUTION } from '@/lib/nyc-zones'

/** NYC metro bounding box (lng, lat) for focus-mode vignette mask. */
const NYC_MASK_OUTER: [number, number][] = [
  [-74.38, 40.48],
  [-73.62, 40.48],
  [-73.62, 40.95],
  [-74.38, 40.95],
  [-74.38, 40.48],
]

export function getVehicleH3Cell(latitude: number, longitude: number): string {
  return h3.latLngToCell(latitude, longitude, NYC_H3_RESOLUTION)
}

export function isVehicleInH3Zone(
  latitude: number,
  longitude: number,
  zoneId: string
): boolean {
  return getVehicleH3Cell(latitude, longitude) === zoneId
}

/** Polygon with hole: dark overlay everywhere except the selected H3 cell. */
export function buildFocusMaskPolygon(focusHex: string): {
  polygon: [number, number][][]
} {
  const holeLatLng = h3.cellToBoundary(focusHex, true)
  const hole: [number, number][] = holeLatLng.map(([lat, lng]) => [lng, lat])
  if (hole.length > 0) {
    hole.push(hole[0])
  }

  return {
    polygon: [NYC_MASK_OUTER, hole],
  }
}

export function getHexFitViewState(
  hex: string,
  options?: { pitch?: number; bearing?: number }
): Pick<MapViewState, 'longitude' | 'latitude' | 'zoom' | 'pitch' | 'bearing'> {
  const boundary = h3.cellToBoundary(hex, true)
  let minLat = Infinity
  let maxLat = -Infinity
  let minLng = Infinity
  let maxLng = -Infinity

  for (const [lat, lng] of boundary) {
    minLat = Math.min(minLat, lat)
    maxLat = Math.max(maxLat, lat)
    minLng = Math.min(minLng, lng)
    maxLng = Math.max(maxLng, lng)
  }

  const latitude = (minLat + maxLat) / 2
  const longitude = (minLng + maxLng) / 2
  const span = Math.max(maxLat - minLat, maxLng - minLng)
  const zoom = Math.min(14.2, Math.max(12.4, 13.6 - Math.log2(span * 100)))

  return {
    longitude,
    latitude,
    zoom,
    pitch: options?.pitch ?? 52,
    bearing: options?.bearing ?? -12,
  }
}

export const DEFAULT_DISPATCH_MAP_VIEW: MapViewState = {
  longitude: -74.006,
  latitude: 40.7128,
  zoom: 11.2,
  pitch: 48,
  bearing: -15,
}
