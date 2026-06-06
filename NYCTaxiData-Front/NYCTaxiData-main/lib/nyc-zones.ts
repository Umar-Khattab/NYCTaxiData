import * as h3 from 'h3-js'

export const NYC_H3_RESOLUTION = 8

const NYC_CENTER = { lat: 40.7128, lng: -74.006 }

/** Stable pool of H3 cells used by the heatmap and dispatch mock data. */
export function getNYCDispatchHexPool(count = 12): string[] {
  const centerHex = h3.latLngToCell(NYC_CENTER.lat, NYC_CENTER.lng, NYC_H3_RESOLUTION)
  return h3.gridDisk(centerHex, 4).slice(0, count)
}

export function formatZoneLabel(hex: string): string {
  return `Sector ${hex.slice(-4).toUpperCase()}`
}
