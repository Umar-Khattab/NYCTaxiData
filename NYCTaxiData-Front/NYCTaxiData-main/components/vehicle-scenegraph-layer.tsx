import { ScenegraphLayer } from '@deck.gl/mesh-layers'
import type { LayerProps } from '@deck.gl/core'
import {
  FleetVehicleData,
  NYC_TAXI_COLOR,
  NYC_TAXI_MODEL_URL,
  TAXI_SCENEGRAPH_SIZE_SCALE,
  taxiOrientation,
} from '@/lib/fleet-vehicle'

const LINEAR = (t: number) => t

export interface VehicleScenegraphLayerOptions {
  id?: string
  data: FleetVehicleData[]
  visible?: boolean
  pickable?: boolean
  transitionMs?: number
  onClick?: LayerProps['onClick']
}

/**
 * High-fidelity 3D fleet layer — classic NYC yellow taxi instances on the map.
 * Reactive to `data` (latitude, longitude, bearing per vehicle).
 */
export function createVehicleScenegraphLayer({
  id = 'vehicle-scenegraph-layer',
  data,
  visible = true,
  pickable = true,
  transitionMs = 280,
  onClick,
}: VehicleScenegraphLayerOptions) {
  return new ScenegraphLayer<FleetVehicleData>({
    id,
    data,
    visible,
    pickable,
    scenegraph: NYC_TAXI_MODEL_URL,
    sizeScale: TAXI_SCENEGRAPH_SIZE_SCALE,
    sizeMinPixels: 6,
    sizeMaxPixels: 48,
    _lighting: 'pbr',
    getPosition: (d) => [d.longitude, d.latitude],
    getOrientation: (d) => taxiOrientation(d.bearing),
    getColor: [...NYC_TAXI_COLOR],
    transitions: {
      getPosition: {
        duration: transitionMs,
        easing: LINEAR,
      },
      getOrientation: {
        duration: transitionMs,
        easing: LINEAR,
      },
    },
    updateTriggers: {
      getPosition: data,
      getOrientation: data,
    },
    onClick,
  })
}
