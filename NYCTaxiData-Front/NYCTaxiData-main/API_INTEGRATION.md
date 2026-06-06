# API Integration Guide

## Quick Start

To connect your backend simulation engine, follow these steps:

### 1. Update API Endpoint

In `stores/use-simulation-engine-store.ts`, replace the mock response with your actual API call:

```typescript
// BEFORE (Mock):
const result = mockSimulationResponse()

// AFTER (Actual API):
const response = await fetch('/api/v1/simulation/run', {
  method: 'POST',
  headers: { 
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${process.env.NEXT_PUBLIC_API_KEY}`
  },
  body: JSON.stringify(input),
})

if (!response.ok) {
  throw new Error(`Simulation API error: ${response.statusText}`)
}

const result = await response.json()
```

### 2. Data Structure Reference

#### Input to Backend

```typescript
{
  "target_datetime": "2026-06-02T14:30",
  "action_type": "reposition",
  "constraints": {
    "max_vehicles": 50,
    "budget_limit": 10000,
    "service_level": 95
  }
}
```

#### Expected Response from Backend

```typescript
{
  "baseline": {
    "profit": 12500,
    "demand_met": 87,
    "fleet_utilization": 72,
    "revenue": 25000,
    "cost": 12500
  },
  "action": {
    "profit": 16200,
    "demand_met": 95,
    "fleet_utilization": 84,
    "revenue": 28500,
    "cost": 12300
  },
  "p50_impact": 3700,
  "p90_impact": 5200,
  "recommendation": "Reposition 12 vehicles to Manhattan Center and Downtown zones. Expected profit improvement: $3.7K (P50)",
  "flowArcs": [
    {
      "from": [-73.95, 40.76],
      "to": [-73.98, 40.78],
      "type": "action",
      "volume": 12
    },
    {
      "from": [-74.0, 40.75],
      "to": [-73.97, 40.77],
      "type": "action",
      "volume": 8
    },
    {
      "from": [-73.98, 40.74],
      "to": [-73.96, 40.79],
      "type": "baseline",
      "volume": 5
    }
  ]
}
```

### 3. Action Type Options

The `action_type` field determines the optimization strategy:

| Type | Description |
|------|-------------|
| `reposition` | Relocate existing vehicles to better zones |
| `expansion` | Add new vehicles/capacity to fleet |
| `none` | Baseline scenario only (no intervention) |

### 4. Constraints Handling

The backend should respect these constraints:

- **max_vehicles**: Don't recommend repositioning more than this number
- **budget_limit**: Total operational cost cannot exceed this amount (in dollars)
- **service_level**: Demand fulfillment target (70-99%, recommend >= 90%)

### 5. Flow Arcs Specification

Flow arcs visualize the suggested repositioning on the map:

```typescript
interface FlowArc {
  from: [longitude: number, latitude: number];  // Origin zone center
  to: [longitude: number, latitude: number];    // Destination zone center
  type: 'baseline' | 'action';                  // Path type
  volume: number;                               // Number of vehicles to move
}
```

**Map Visualization:**
- **Green lines** (`type: 'action'`)  → Recommended intervention paths
- **Indigo lines** (`type: 'baseline'`) → Current baseline paths

### 6. Error Handling

The UI automatically handles errors. In your backend, return appropriate HTTP status codes:

```
200 - Success
400 - Invalid input parameters
401 - Unauthorized
429 - Rate limit exceeded
500 - Server error
```

Error messages will be displayed in the Cockpit status panel.

### 7. Testing with Mock Data

To test the UI without backend:

1. Modify `mockSimulationResponse()` in `use-simulation-engine-store.ts`
2. The mock currently returns realistic data
3. Leave it as-is during development, replace when backend ready

### 8. Performance Tips

- Cache frequently requested time windows
- Pre-compute baseline scenarios (they don't change)
- For action scenarios, use incremental computation
- Return P50/P90 estimates (use percentile-based approach)

## Backend Implementation Example

### Python/Flask Example

```python
@app.post('/api/v1/simulation/run')
def run_simulation():
    input_data = request.json
    
    # Extract parameters
    target_time = input_data['target_datetime']
    action_type = input_data['action_type']
    constraints = input_data['constraints']
    
    # Run baseline scenario
    baseline = compute_baseline(target_time)
    
    # Run action scenario
    if action_type != 'none':
        action = compute_action(
            target_time, 
            action_type, 
            constraints
        )
        flow_arcs = extract_flow_arcs(action)
    else:
        action = baseline
        flow_arcs = []
    
    # Calculate impacts
    p50, p90 = compute_profit_impact(baseline, action)
    
    # Generate recommendation
    rec = generate_recommendation(action_type, action, constraints)
    
    return {
        'baseline': baseline.to_dict(),
        'action': action.to_dict(),
        'p50_impact': p50,
        'p90_impact': p90,
        'recommendation': rec,
        'flowArcs': flow_arcs
    }
```

### Node.js/Express Example

```typescript
app.post('/api/v1/simulation/run', async (req, res) => {
  try {
    const input = req.body
    
    // Validate input
    if (!input.target_datetime || !input.action_type) {
      return res.status(400).json({ error: 'Missing required fields' })
    }
    
    // Compute scenarios in parallel
    const [baseline, action] = await Promise.all([
      computeBaseline(input.target_datetime),
      input.action_type === 'none' 
        ? null 
        : computeAction(input)
    ])
    
    const result = {
      baseline,
      action: action || baseline,
      p50_impact: calculateImpact(baseline, action, 0.5),
      p90_impact: calculateImpact(baseline, action, 0.9),
      recommendation: generateRecommendation(action),
      flowArcs: extractFlowArcs(action)
    }
    
    res.json(result)
  } catch (error) {
    res.status(500).json({ error: error.message })
  }
})
```

## Debugging

### Check API Calls

In browser DevTools (Network tab):
1. Click "Run Simulation"
2. Look for POST request to `/api/v1/simulation/run`
3. Verify request body contains correct parameters
4. Check response status and payload

### Check State Updates

In React DevTools:
1. Inspect `useSimulationEngineStore`
2. Watch for `isLoading` → `true` when simulation starts
3. Watch for `result` → populated when API responds
4. Watch for `error` → set if API fails

### Logs

Add debug logging in `runSimulation()`:

```typescript
runSimulation: async (input) => {
  console.log('Starting simulation with:', input)
  set({ isLoading: true, error: null })
  try {
    const response = await fetch(...)
    console.log('API response:', response)
    const result = await response.json()
    console.log('Parsed result:', result)
    set({ result, isLoading: false })
  } catch (err) {
    console.error('Simulation error:', err)
    set({ error: err.message, isLoading: false })
  }
}
```

## Environment Configuration

Add to `.env.local`:

```
NEXT_PUBLIC_API_URL=https://api.example.com
NEXT_PUBLIC_API_KEY=your-api-key
SIMULATION_TIMEOUT=30000
```

Then use in your API call:

```typescript
const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:3000'
const response = await fetch(`${apiUrl}/api/v1/simulation/run`, {
  ...
})
```

## Deployment Checklist

- [ ] Backend API deployed and accessible
- [ ] CORS headers configured correctly
- [ ] API authentication/authorization working
- [ ] Rate limiting implemented
- [ ] Error responses return proper status codes
- [ ] Response payload matches SimulationResult schema
- [ ] Flow arcs coordinates are valid (lon: -180 to 180, lat: -90 to 90)
- [ ] P50/P90 calculations are statistically sound
- [ ] Performance acceptable (<5s for most requests)
