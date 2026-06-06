import { create } from 'zustand'

interface LayoutState {
  layouts: Record<string, any>
  saveLayout: (breakpoint: string, layout: any) => void
  loadLayout: (breakpoint: string) => any | null
  resetLayout: () => void
}

const STORAGE_KEY = 'analytics-grid-layouts'

const defaultLayouts = {
  lg: [
    { i: 'cockpit', x: 0, y: 0, w: 12, h: 4, minW: 10, minH: 3 },
    { i: 'map', x: 0, y: 4, w: 8, h: 20, minW: 6, minH: 16 },
    { i: 'input', x: 8, y: 4, w: 4, h: 12, minW: 4, minH: 10 },
    { i: 'comparison', x: 8, y: 16, w: 4, h: 8, minW: 4, minH: 8 },
    { i: 'kpi', x: 0, y: 24, w: 8, h: 8, minW: 6, minH: 8 },
    { i: 'recommendation', x: 8, y: 24, w: 4, h: 8, minW: 4, minH: 8 },
  ],
  md: [
    { i: 'cockpit', x: 0, y: 0, w: 10, h: 4, minW: 8, minH: 3 },
    { i: 'map', x: 0, y: 4, w: 10, h: 18, minW: 6, minH: 14 },
    { i: 'input', x: 0, y: 22, w: 5, h: 10, minW: 5, minH: 8 },
    { i: 'comparison', x: 5, y: 22, w: 5, h: 10, minW: 5, minH: 8 },
    { i: 'kpi', x: 0, y: 32, w: 10, h: 8, minW: 6, minH: 8 },
    { i: 'recommendation', x: 0, y: 40, w: 10, h: 8, minW: 6, minH: 8 },
  ],
  sm: [
    { i: 'cockpit', x: 0, y: 0, w: 6, h: 4, minW: 6, minH: 3 },
    { i: 'map', x: 0, y: 4, w: 6, h: 16, minW: 6, minH: 12 },
    { i: 'input', x: 0, y: 20, w: 6, h: 10, minW: 6, minH: 8 },
    { i: 'comparison', x: 0, y: 30, w: 6, h: 10, minW: 6, minH: 8 },
    { i: 'kpi', x: 0, y: 40, w: 6, h: 8, minW: 6, minH: 8 },
    { i: 'recommendation', x: 0, y: 48, w: 6, h: 8, minW: 6, minH: 8 },
  ],
  xs: [
    { i: 'cockpit', x: 0, y: 0, w: 4, h: 4, minW: 4, minH: 3 },
    { i: 'map', x: 0, y: 4, w: 4, h: 14, minW: 4, minH: 10 },
    { i: 'input', x: 0, y: 18, w: 4, h: 10, minW: 4, minH: 8 },
    { i: 'comparison', x: 0, y: 28, w: 4, h: 10, minW: 4, minH: 8 },
    { i: 'kpi', x: 0, y: 38, w: 4, h: 8, minW: 4, minH: 8 },
    { i: 'recommendation', x: 0, y: 46, w: 4, h: 8, minW: 4, minH: 8 },
  ],
  xxs: [
    { i: 'cockpit', x: 0, y: 0, w: 2, h: 4, minW: 2, minH: 3 },
    { i: 'map', x: 0, y: 4, w: 2, h: 14, minW: 2, minH: 10 },
    { i: 'input', x: 0, y: 18, w: 2, h: 10, minW: 2, minH: 8 },
    { i: 'comparison', x: 0, y: 28, w: 2, h: 10, minW: 2, minH: 8 },
    { i: 'kpi', x: 0, y: 38, w: 2, h: 8, minW: 2, minH: 8 },
    { i: 'recommendation', x: 0, y: 46, w: 2, h: 8, minW: 2, minH: 8 },
  ],
}

export const useLayoutStore = create<LayoutState>((set, get) => ({
  layouts: loadLayoutsFromStorage(),

  saveLayout: (breakpoint, layout) => {
    set((state) => {
      const updated = {
        ...state.layouts,
        [breakpoint]: layout,
      }
      localStorage.setItem(STORAGE_KEY, JSON.stringify(updated))
      return { layouts: updated }
    })
  },

  loadLayout: (breakpoint) => {
    const state = get()
    return state.layouts[breakpoint] || defaultLayouts[breakpoint as keyof typeof defaultLayouts] || null
  },

  resetLayout: () => {
    localStorage.removeItem(STORAGE_KEY)
    set({ layouts: defaultLayouts })
  },
}))

function loadLayoutsFromStorage(): Record<string, any> {
  if (typeof window === 'undefined') return defaultLayouts

  try {
    const stored = localStorage.getItem(STORAGE_KEY)
    return stored ? JSON.parse(stored) : defaultLayouts
  } catch (error) {
    console.error('Failed to load layouts from storage:', error)
    return defaultLayouts
  }
}
