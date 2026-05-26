import type { SimulationStatusResponse } from '../types/simulation'

interface ControlBarProps {
  status: SimulationStatusResponse | null
  isRunning: boolean
  isPaused: boolean
  onPlay: () => void
  onPause: () => void
  onStop: () => void
  onSpeedChange: (speed: number) => void
}

export function ControlBar({
  status,
  isRunning,
  isPaused,
  onPlay,
  onPause,
  onStop,
  onSpeedChange,
}: ControlBarProps) {
  const speed = status?.speedFactor ?? 60
  const simulatedTime =
    status?.simulatedTime && new Date(status.simulatedTime).toLocaleString()

  return (
    <div className="control-bar">
      <div className="control-group">
        <button className="primary" type="button" onClick={onPlay}>
          {isRunning && !isPaused ? 'Restart' : 'Play'}
        </button>
        <button type="button" onClick={onPause} disabled={!isRunning}>
          {isPaused ? 'Resume' : 'Pause'}
        </button>
        <button type="button" onClick={onStop} disabled={!isRunning}>
          Stop
        </button>
      </div>
      <div className="control-group">
        <label htmlFor="speed">Speed: {Math.round(speed)}x</label>
        <input
          id="speed"
          type="range"
          min={1}
          max={200}
          value={speed}
          onChange={(event) => onSpeedChange(Number(event.target.value))}
        />
      </div>
      <div className="control-group status">
        <span>Status: {status?.status ?? 'Idle'}</span>
        <span>Simulated time: {simulatedTime ?? '--'}</span>
        <span>Hour: {status?.currentHour ?? 0}</span>
      </div>
    </div>
  )
}
