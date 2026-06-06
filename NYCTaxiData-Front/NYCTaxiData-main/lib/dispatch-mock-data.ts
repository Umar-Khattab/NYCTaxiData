/**
 * dispatch-mock-data.ts
 *
 * Previously contained random-data generators.
 * Now re-exports the live API utilities so all import sites continue to work
 * without changes, while all data comes from the real fleet API.
 */

export {
  DISPATCH_ZONE_OPTIONS,
  DISPATCH_ZONE_HEXES,
  defaultAutoDispatchRules,
  generateDispatchRequests,
} from '@/lib/dispatch-data'
