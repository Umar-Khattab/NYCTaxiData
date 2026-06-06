/** NYC fleet taxi color — classic medallion yellow */
export const NYC_TAXI_COLOR = [253, 197, 0, 255] as const

export const NYC_TAXI_MODEL_URL = '/models/nyc-yellow-taxi.glb'

/**
 * H3 resolution-8 hex edge ≈ 461 m. Taxi length ≈ 4.6 m (~1% of cell width).
 * sizeScale is applied to the glTF asset (modeled in meters).
 */
export const TAXI_SCENEGRAPH_SIZE_SCALE = 2.75

export interface FleetVehicleData {
  id: string
  latitude: number
  longitude: number
  /** Heading in degrees: 0 = north, clockwise (navigation bearing). */
  bearing: number
}

/** Linear interpolation for scalars. */
export function lerp(a: number, b: number, t: number): number {
  return a + (b - a) * t
}

/** Shortest-path linear interpolation for compass bearings (degrees). */
export function lerpBearing(from: number, to: number, t: number): number {
  let delta = ((to - from + 540) % 360) - 180
  return (from + delta * t + 360) % 360
}

/** Navigation bearing from point A to B (degrees, 0 = north). */
export function bearingDegrees(
  lng1: number,
  lat1: number,
  lng2: number,
  lat2: number
): number {
  const toRad = Math.PI / 180
  const dLng = (lng2 - lng1) * toRad
  const lat1r = lat1 * toRad
  const lat2r = lat2 * toRad
  const y = Math.sin(dLng) * Math.cos(lat2r)
  const x =
    Math.cos(lat1r) * Math.sin(lat2r) -
    Math.sin(lat1r) * Math.cos(lat2r) * Math.cos(dLng)
  return (Math.atan2(y, x) * (180 / Math.PI) + 360) % 360
}

/** ScenegraphLayer orientation: [pitch, yaw, roll] in degrees. */
export function taxiOrientation(bearing: number): [number, number, number] {
  return [0, -bearing, 90]
}
