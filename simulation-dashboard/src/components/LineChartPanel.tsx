import {
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
  CartesianGrid,
  Legend,
} from 'recharts'

interface LineChartPanelProps {
  data: { time: string; demand: number; revenue: number }[]
}

export function LineChartPanel({ data }: LineChartPanelProps) {
  return (
    <div className="card chart-panel">
      <header>
        <h3>Demand & Revenue Trend</h3>
        <span>Live aggregated metrics</span>
      </header>
      <div className="chart-container">
        <ResponsiveContainer width="100%" height={260}>
          <LineChart data={data}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="time" />
            <YAxis />
            <Tooltip />
            <Legend />
            <Line
              type="monotone"
              dataKey="demand"
              stroke="#6366f1"
              strokeWidth={2}
              isAnimationActive
              animationDuration={400}
            />
            <Line
              type="monotone"
              dataKey="revenue"
              stroke="#22c55e"
              strokeWidth={2}
              isAnimationActive
              animationDuration={400}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}
