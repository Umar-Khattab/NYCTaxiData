'use client'

import * as React from 'react'
import {
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query'

/**
 * A single QueryClient instance created once per browser session.
 * Kept outside the component so it is never re-created on re-renders.
 */
function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // Data stays "fresh" for 30 s — avoids refetching on every focus event
        staleTime: 30_000,
        // Keep unused data in cache for 5 minutes
        gcTime: 5 * 60_000,
        // Show stale data while revalidating in background
        refetchOnWindowFocus: false,
        // Retry failed requests once before surfacing the error
        retry: 1,
      },
    },
  })
}

let browserQueryClient: QueryClient | undefined

function getQueryClient() {
  if (typeof window === 'undefined') {
    // Server: always create a fresh client (never shared between requests)
    return makeQueryClient()
  }
  // Browser: reuse the existing client, or create one if this is the first call
  if (!browserQueryClient) browserQueryClient = makeQueryClient()
  return browserQueryClient
}

interface ProvidersProps {
  children: React.ReactNode
}

export function Providers({ children }: ProvidersProps) {
  /**
   * NOTE: Avoid useState here.
   * If React suspends during the initial render before the state is set,
   * a new QueryClient would be created on every re-render.
   * getQueryClient() handles this safely.
   */
  const queryClient = getQueryClient()

  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  )
}
