# Future Mobility Simulation & Decision Hub - Refactoring Guide

## Overview

The Predictive Analytics page has been refactored into a comprehensive **Future Mobility Simulation & Decision Hub** with:
- Interactive 3D map visualization with Flow Arcs and Heatmap layers
- Drag-and-drop dashboard with responsive grid layout
- Persistent layout storage (localStorage)
- Simulation input controls and counterfactual analysis
- Baseline vs Action comparison charts
- KPI metrics tracking
- Recommended action cards with export functionality

## Architecture

### State Management (Zustand Stores)

#### 1. **useAnalyticsSimulationStore** (`stores/use-analytics-simulation-store.ts`)
Manages simulation vehicle data and prediction snapshots.

**Key State:**
- `selectedStep`: Current simulation timestep (0-47)
- `timeLabels`: Array of time labels for each step
- `simulationData`: Contains vehicles and prediction snapshots
- `setSelectedStep()`: Update current simulation step
- `initializeSimulation()`: Initialize with custom data

**Helper Functions:**
- `getSimulationVehicles(step)`: Generate mock vehicles for a given step
- `getPredictionSnapshot(step)`: Get demand/supply data for a step

#### 2. **useSimulationEngineStore** (`stores/use-simulation-engine-store.ts`)
Handles simulation input parameters and results from the backend API.

**Key State:**
- `input`: SimulationInput (target_datetime, action_type, constraints)
- `result`: SimulationResult (baseline & action metrics, flow arcs, recommendations)
- `isLoading`: Boolean flag for simulation execution
- `error`: Error messages from API

**Key Methods:**
- `setInput(updates)`: Update simulation input parameters
- `runSimulation(input)`: Call backend API and update results
- `clearResult()`: Reset simulation results
- `setError(error)`: Set error state

**API Integration Point:**
```typescript
// In useSimulationEngineStore.runSimulation()
// Replace the mock response with:
const response = await fetch('/api/simulation/run', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(input),
})
const result = await response.json()
```

#### 3. **useLayoutStore** (`stores/use-layout-store.ts`)
Persists grid layout configuration to localStorage.

**Key Methods:**
- `saveLayout(breakpoint, layout)`: Save layout for a breakpoint
- `loadLayout(breakpoint)`: Load saved layout
- `resetLayout()`: Reset to default layout

## Components

### 1. **SimulationMap** (`components/simulation-map.tsx`)
Enhanced Deck.gl map showing:
- Vehicle positions (ScatterplotLayer)
- Demand heatmap visualization (HeatmapLayer)
- Baseline paths (green Path layer)
- Action intervention paths (indigo Path layer)

**Props:**
- `vehicles`: Array of SimulationVehicle objects
- `simulationResult`: Optional SimulationResult with flow arcs
- `height`: CSS height class (default: 'h-full')

**Features:**
- Auto-resizes when widget is resized
- Displays flow arcs colored by type (baseline vs action)
- Interactive tooltips on hover

### 2. **SimulationCockpit** (`components/simulation-cockpit.tsx`)
Control panel showing system status and reset functionality.

**Props:**
- `isLoading`: Loading state during simulation
- `onReset`: Callback to reset layout
- `status`: 'ready' | 'running' | 'completed' | 'error'
- `lastRunTime`: ISO timestamp of last simulation

**Displays:**
- Simulation status with animations
- System health indicators (data pipeline, API, memory)
- Quick help text

### 3. **SimulationInputPanel** (`components/simulation-input-panel.tsx`)
User input controls for simulation parameters.

**Props:**
- `input`: Current simulation input configuration
- `onInputChange`: Callback when user changes inputs
- `onSimulate`: Callback when user clicks "Run Simulation"
- `isLoading`: Loading state

**Inputs:**
- Target simulation datetime
- Action type (reposition, expansion, none)
- Constraints: max vehicles, budget limit, service level SLA

### 4. **ComparisonCharts** (`components/comparison-charts.tsx`)
Side-by-side bar charts comparing baseline vs action scenarios.

**Metrics Displayed:**
- Profit comparison
- Revenue comparison
- Cost comparison

**Props:**
- `result`: SimulationResult from backend
- `isLoading`: Loading state

### 5. **KPIComparison** (`components/kpi-comparison.tsx`)
KPI metrics showing improvements from action vs baseline.

**Metrics Tracked:**
- Unmet Demand (percentage of unfulfilled orders)
- Fleet Utilization (percentage of active vehicles)

**Features:**
- Color-coded improvement badges (green for improvement, red for decline)
- Trend indicators

### 6. **RecommendedAction** (`components/recommended-action.tsx`)
Decision support card with P50/P90 profit impact and export options.

**Features:**
- Natural language recommendation text
- P50 (median) and P90 (percentile) profit impact
- Export buttons for JSON, CSV, PDF formats
- Disclaimer about verification with operations team

## Layout Configuration

The dashboard uses responsive grid layouts defined in `useLayoutStore`:

```typescript
// Large screens (lg: 1200px+)
lg: [
  { i: 'cockpit', x: 0, y: 0, w: 12, h: 4 },     // Full width header
  { i: 'map', x: 0, y: 4, w: 8, h: 20 },         // Left column, main map
  { i: 'input', x: 8, y: 4, w: 4, h: 12 },       // Right column, inputs
  { i: 'comparison', x: 8, y: 16, w: 4, h: 8 },  // Right column, charts
  { i: 'kpi', x: 0, y: 24, w: 8, h: 8 },         // Bottom left, KPIs
  { i: 'recommendation', x: 8, y: 24, w: 4, h: 8 } // Bottom right, recommendation
]
// ... similar layouts for md, sm, xs, xxs breakpoints
```

**Layout Persistence:**
- Layouts are saved to `localStorage` under key `'analytics-grid-layouts'`
- Users can drag/drop and resize widgets
- Layout persists across page refreshes
- "Reset Layout" button restores default configuration

## Data Flow

### Simulation Execution Flow

```
User Input (SimulationInputPanel)
    ↓
setInput() → Updates useSimulationEngineStore
    ↓
handleSimulate() → Calls runSimulation()
    ↓
[isLoading = true]
    ↓
fetch('/api/simulation/run', input)
    ↓
Backend Analysis
- Calculate baseline scenario
- Calculate action scenario with constraints
- Compute KPI changes
- Generate flow arcs for repositioning paths
- Calculate P50/P90 profit impact
    ↓
[Backend returns SimulationResult]
    ↓
Store updates: result, isLoading=false
    ↓
Reactively re-render:
- ComparisonCharts (baseline vs action)
- KPIComparison (unmet demand, fleet util)
- RecommendedAction (recommendation & export)
- SimulationMap (flow arcs visualization)
```

## API Integration

### Expected Endpoint

**POST `/api/simulation/run`**

**Request Body:**
```typescript
interface SimulationInput {
  target_datetime: string;        // ISO datetime string
  action_type: 'reposition' | 'expansion' | 'none';
  constraints: {
    max_vehicles: number;         // Max vehicles for action
    budget_limit: number;         // Budget in dollars
    service_level: number;        // Target fulfillment % (70-99)
  };
}
```

**Response Body:**
```typescript
interface SimulationResult {
  baseline: {
    profit: number;
    demand_met: number;           // % of orders fulfilled
    fleet_utilization: number;    // % vehicles active
    revenue: number;
    cost: number;
  };
  action: {
    profit: number;
    demand_met: number;
    fleet_utilization: number;
    revenue: number;
    cost: number;
  };
  p50_impact: number;             // Median profit improvement
  p90_impact: number;             // 90th percentile improvement
  recommendation: string;         // Natural language recommendation
  flowArcs: Array<{
    from: [longitude, latitude];
    to: [longitude, latitude];
    type: 'baseline' | 'action';
    volume: number;               // Number of vehicles
  }>;
}
```

### Backend Implementation Notes

1. **Baseline Scenario**: Current fleet state without intervention
2. **Action Scenario**: Optimized fleet state with repositioning/expansion
3. **Constraints**: Must respect max_vehicles, budget_limit, and service_level
4. **Flow Arcs**: Origin-destination lines showing suggested repositioning
5. **P50/P90**: Percentile-based impact estimates for decision uncertainty

### Mock Data

Currently, `useSimulationEngineStore` returns mock data (function `mockSimulationResponse()`). Replace this with your actual API call.

## Styling

### Design System

- **Dark Glassmorphism Theme**: Dark background with semi-transparent frosted glass effect
- **Color Palette**:
  - Primary: Violet/Purple (`from-violet-500`)
  - Accent: Cyan/Emerald (`from-cyan-600 to-emerald-600`)
  - Status: Green (success), Yellow (warning), Red (error), Amber (info)
- **Typography**: SF Pro Display with careful hierarchy
- **Spacing**: 16px base unit with tailwind scales

### Component Classes

All components use consistent styling:
- `rounded-3xl`: Aggressive border radius
- `border border-white/10`: Subtle borders with transparency
- `bg-slate-950/40`: Semi-transparent dark backgrounds
- `shadow-[0_0_40px_rgba(124,58,237,0.12)]`: Purple glow shadows
- `backdrop-blur-2xl`: Strong blur effect

## Performance Considerations

### Optimization Strategies

1. **Deck.gl Map**:
   - Uses ResizeObserver to auto-resize on widget drag
   - Only recomputes layers when vehicles or flowArcs change
   - Heatmap layer at 50% intensity to reduce GPU load

2. **Grid Layout**:
   - `useCSSTransforms={true}` for hardware acceleration
   - `compactType="vertical"` to optimize layout shifts

3. **React Rendering**:
   - useMemo on vehicles calculation
   - Responsive containers debounce chart redraws

### Scalability

For production use with real data:
1. Implement virtual scrolling in KPI comparison
2. Debounce layout change callbacks
3. Add data pagination for large flow arc datasets
4. Consider WebGL for chart rendering (vs Canvas)

## Testing Checklist

- [ ] Drag and drop widgets - verify layout persists
- [ ] Resize widgets - verify Deck.gl and charts adapt
- [ ] Reset layout button - verify return to defaults
- [ ] Input controls - verify state updates correctly
- [ ] Run simulation - verify API call and results display
- [ ] Export buttons - verify file downloads
- [ ] Responsive design - test on mobile, tablet, desktop
- [ ] Dark theme - verify all text is readable

## Future Enhancements

1. **Real-time Updates**: WebSocket connection for live simulation updates
2. **Historical Comparisons**: Compare current simulation against past runs
3. **Custom Scenarios**: Save and load user-defined input scenarios
4. **Advanced Filtering**: Filter vehicles/zones by attributes
5. **3D Fleet Visualization**: Add altitude/speed dimensions to scatter plot
6. **Prediction Confidence**: Show confidence bands on charts
7. **Decision History**: Log all simulations and recommendations
8. **Integration with Decision System**: Auto-execute recommended actions

## Troubleshooting

### Map Not Rendering
- Verify Deck.gl dependencies installed: `npm install @deck.gl/core @deck.gl/react @deck.gl/layers`
- Check browser console for WebGL errors
- Ensure container has valid height

### Layout Not Persisting
- Check browser localStorage is enabled
- Verify `useLayoutStore` is properly initialized
- Clear localStorage: `localStorage.removeItem('analytics-grid-layouts')`

### Simulation Not Running
- Verify backend API endpoint URL
- Check network tab for API errors
- Review backend error logs
- Ensure CORS headers are configured correctly

### Charts Not Updating
- Verify `result` prop is being passed correctly
- Check Recharts installation: `npm install recharts`
- Monitor React render cycles in DevTools Profiler

## File Structure

```
app/
  analytics/
    page.tsx                    # Main page component
components/
  simulation-map.tsx            # Deck.gl 3D map with flow arcs
  simulation-cockpit.tsx        # System status & controls
  simulation-input-panel.tsx    # Simulation parameters
  comparison-charts.tsx         # Baseline vs action comparison
  kpi-comparison.tsx           # KPI metrics display
  recommended-action.tsx        # Recommendation & export
stores/
  use-analytics-simulation-store.ts  # Vehicle & prediction data
  use-simulation-engine-store.ts     # Simulation input & results
  use-layout-store.ts                # Grid layout persistence
lib/
  dispatch-types.ts             # Type definitions
  utils.ts                      # Utility functions
```
