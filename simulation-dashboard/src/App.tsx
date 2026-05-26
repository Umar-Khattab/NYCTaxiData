import './App.css'
import { ControlBar } from './components/ControlBar'
import { HeatmapPanel } from './components/HeatmapPanel'
import { LineChartPanel } from './components/LineChartPanel'
import { StatsPanel } from './components/StatsPanel'
import { ZoneComparePanel } from './components/ZoneComparePanel'
import { useSimulation } from './hooks/useSimulation'

const startOfDay = new Date()
startOfDay.setHours(0, 0, 0, 0)

const DEFAULT_START = {
  durationHours: 24,
  speedFactor: 60,
  totalDrivers: 300,
  zoneCount: 30,
  startTime: startOfDay.toISOString(),
}

function App() {
  const {
    status,
    latestTick,
    aggregateSeries,
    zoneHistory,
    availableZones,
    isRunning,
    isPaused,
    startSimulation,
    sendControl,
  } = useSimulation()

  const handlePlay = async () => {
    if (isRunning && !isPaused) {
      await startSimulation(DEFAULT_START)
      return
    }

    if (isPaused) {
      await sendControl('resume')
      return
    }

    await startSimulation(DEFAULT_START)
  }

  const handlePause = async () => {
    if (!isRunning) return
    await sendControl(isPaused ? 'resume' : 'pause')
  }

  const handleStop = async () => {
    await sendControl('stop')
  }

  const handleSpeedChange = async (speed: number) => {
    await sendControl('speed', speed)
  }

  return (
    <div className="dashboard">
      <header className="page-header">
        <div>
          <h1>NYC Taxi Simulation Dashboard</h1>
          <p>Faster-than-real-time operational simulation</p>
        </div>
        <span className="badge">Live Demo</span>
      </header>

      <ControlBar
        status={status}
        isRunning={isRunning}
        isPaused={isPaused}
        onPlay={handlePlay}
        onPause={handlePause}
        onStop={handleStop}
        onSpeedChange={handleSpeedChange}
      />

      <section className="grid">
        <StatsPanel tick={latestTick} />
        <LineChartPanel data={aggregateSeries} />
        <HeatmapPanel zones={latestTick?.zones ?? []} />
        <ZoneComparePanel
          availableZones={availableZones}
          zoneHistory={zoneHistory}
        />
      </section>
    </div>
  )
}

export default App
