// في ملف fleetApi.ts
const BASE_URL = "https://zonax.runasp.net/api/v1";
const TOKEN = "Bearer •••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••••──";

const getHeaders = (includeToken = true) => {
  const headers = new Headers();
  headers.append("Content-Type", "application/json");
  if (includeToken) {
    headers.append("Authorization", TOKEN);
  }
  return headers;
};

export interface ZoneStateReposition {
  zoneId: number;
  currentDrivers: number;
  predictedDemand: number;
  currentDemand: number;
}

export interface ZoneStateProfit {
  zone_id: number;
  current_drivers: number;
  allow_as_source: boolean;
  allow_as_target: boolean;
  is_event_zone: boolean;
  is_airport_zone: boolean;
  hour: number;
  day_of_week: number;
  is_weekend: number;
  temp_c: number;
  rain_mm: number;
  is_rain: number;
  weather_code: number;
  is_holiday: number;
  lag_1_6h: number;
  lag_2_6h: number;
  lag_4_6h: number;
  rolling_mean_24h: number;
  rev_lag_1_6h: number;
  rev_lag_1_week: number;
  rev_rolling_mean_7d: number;
  rev_rolling_mean_30d: number;
  avg_fare: number;
  tip_rate: number;
  pickup_count: number;
  dropoff_count: number;
  net_flow: number;
  activity_ratio: number;
  lag_1_pickup: number;
  lag_1_dropoff: number;
  lag_1_net_flow: number;
}

export interface ThresholdsConfig {
  surgeMultipliers: {
    normal: number;
    elevated: number;
    critical: number;
  };
  dispatchRadiusKm: {
    normal: number;
    elevated: number;
    critical: number;
  };
}

export interface OfflineTrip {
  localTripId: string;
  pickupLocationId: number;
  dropoffLocationId: number;
  startedAt: string;
  endedAt: string;
  actualFare: number;
}

export interface ManualDispatchPayload {
  driverId: string;
  pickupZoneId: number;
  dropoffZoneId: number;
  passengerName: string;
  passengerPhone: string;
  priority: "NORMAL" | "HIGH" | "CRITICAL";
  smartRoutingEnabled: boolean;
  tripId: string | null;
}

export interface DriverTripStatusPayload {
  driverId: string;
  status: string;
  currentLat: number;
  currentLng: number;
}

export interface StartTripPayload {
  tripId: number;
  driverId: string;
  pickupLocationId: number;
  dropoffLocationId: number;
}

export interface EndTripPayload {
  tripId: number;
  farePerMinute: number;
  baseFare: number;
  surgeMultiplier: number;
}

export interface CreateTripPayload {
  driverId: string;
  pickupLocationId: number;
  dropoffLocationId: number;
  fareAmount: number;
  tipAmount: number;
}

export interface UpdateTripPayload {
  tripId: number;
  fareAmount: number;
  tipAmount: number;
  processStatus: string;
}

export const fleetApi = {
  auth: {
    getProfile: async (phoneNumber: string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/auth/profile/${phoneNumber}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch profile data');
      return response.json();
    },

    login: async (credentials: { phoneNumber: string; password: string }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(false),
        body: JSON.stringify(credentials),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/auth/login`, requestOptions);
      if (!response.ok) throw new Error('Login failed');
      return response.json();
    },

    registerDriver: async (driverData: any) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(false),
        body: JSON.stringify(driverData),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/auth/register/driver`, requestOptions);
      if (!response.ok) throw new Error('Driver registration failed');
      return response.json();
    },

    registerManager: async (managerData: any) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(false),
        body: JSON.stringify(managerData),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/auth/register/manager`, requestOptions);
      if (!response.ok) throw new Error('Manager registration failed');
      return response.json();
    },

    sendOtp: async (phoneNumber: string) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(false),
        body: JSON.stringify({ phoneNumber }),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/auth/otp/send`, requestOptions);
      if (!response.ok) throw new Error('Failed to send OTP');
      return response.json();
    },

    verifyOtp: async (otpData: { phoneNumber: string; otpCode: string }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(false),
        body: JSON.stringify(otpData),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/auth/otp/verify`, requestOptions);
      if (!response.ok) throw new Error('OTP verification failed');
      return response.json();
    },

    resetPassword: async (resetData: { resetToken: string; newPassword: string }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(false),
        body: JSON.stringify(resetData),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/auth/password/reset`, requestOptions);
      if (!response.ok) throw new Error('Password reset failed');
      return response.json();
    },

    refreshToken: async (oldToken: string) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(false),
        body: JSON.stringify({ oldToken }),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/auth/token/refresh`, requestOptions);
      if (!response.ok) throw new Error('Failed to refresh token');
      return response.json();
    }
  },
  
  admin: {
    getStats: async (fromDate = "2024-01-01", toDate = "2024-01-31") => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/admin/stats?from=${fromDate}&to=${toDate}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch admin stats');
      return response.json();
    },

    aggregateByDate: async (date: string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/admin/aggregate/${date}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to aggregate for date: ${date}`);
      return response.json();
    },

    aggregateToday: async () => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/admin/aggregate/today`, requestOptions);
      if (!response.ok) throw new Error('Failed to run aggregate today');
      return response.json();
    }
  },

  zones: {
    getAll: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch real-world taxi zones');
      return response.json();
    },

    getMetadata: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/metadata`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch zones metadata');
      return response.json();
    },

    getStatistics: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/statistics`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch global zones statistics');
      return response.json();
    },

    getById: async (zoneId: number | string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/${zoneId}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch details for zone ${zoneId}`);
      return response.json();
    },

    getZoneStatistics: async (zoneId: number | string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/${zoneId}/statistics`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch statistics for zone ${zoneId}`);
      return response.json();
    },

    getInsights: async (zoneId: number | string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/${zoneId}/insights`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch insights for zone ${zoneId}`);
      return response.json();
    },

    getHeatmap: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/heatmap`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch zones heatmap data');
      return response.json();
    },

    compareMultiple: async (zoneIds: number[]) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const queryParams = zoneIds.map(id => `zoneIds=${id}`).join('&');
      const response = await fetch(`${BASE_URL}/zones/compare?${queryParams}`, requestOptions);
      if (!response.ok) throw new Error('Failed to compare zones');
      return response.json();
    },

    getRecommended: async (limit = 10) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/recommended?limit=${limit}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch recommended zones');
      return response.json();
    },

    getTrends: async (zoneId: number | string, trendType = 'hourly') => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/trends?zoneId=${zoneId}&trendType=${trendType}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch trends for zone ${zoneId}`);
      return response.json();
    },

    getHistory: async (zoneId: number | string, startDate: string, endDate: string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(
        `${BASE_URL}/zones/history?zoneId=${zoneId}&startDate=${encodeURIComponent(startDate)}&endDate=${encodeURIComponent(endDate)}`,
        requestOptions
      );
      if (!response.ok) throw new Error(`Failed to fetch history for zone ${zoneId}`);
      return response.json();
    },

    getPeakHours: async (zoneId: number | string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/peak-hours?zoneId=${zoneId}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch peak hours for zone ${zoneId}`);
      return response.json();
    },

    getDriverDistribution: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/driver-distribution`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch driver distribution across zones');
      return response.json();
    },

    getTopDemand: async (limit = 10) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/top-demand?limit=${limit}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch top demand zones');
      return response.json();
    },

    getTopRevenue: async (limit = 10) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/top-revenue?limit=${limit}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch top revenue zones');
      return response.json();
    },

    getHighStockout: async (limit = 10) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/zones/high-stockout?limit=${limit}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch high stockout zones');
      return response.json();
    }
  },

  drivers: {
    getEarnings: async (driverId: string) => {
      const requestOptions = {
        method: 'GET',
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/drivers/earnings?driverId=${driverId}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch earnings for driver ${driverId}`);
      return response.json();
    },

    getAnalytics: async (driverId: string) => {
      const requestOptions = {
        method: 'GET',
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/drivers/analytics?driverId=${driverId}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch analytics for driver ${driverId}`);
      return response.json();
    },

    getFilteredDrivers: async (status = "Available", zoneId: number | string = 132, pageNumber = 1, pageSize = 10) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(
        `${BASE_URL}/drivers?status=${status}&zoneId=${zoneId}&pageNumber=${pageNumber}&pageSize=${pageSize}`,
        requestOptions
      );
      if (!response.ok) throw new Error('Failed to fetch filtered drivers list');
      return response.json();
    },

    getActiveDrivers: async (pageNumber = 1, pageSize = 10) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/drivers/active?pageNumber=${pageNumber}&pageSize=${pageSize}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch active drivers list');
      return response.json();
    },

    getById: async (driverId: string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/drivers/${driverId}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch profile details for driver ${driverId}`);
      return response.json();
    },

    getShiftStats: async (driverId: string, shiftStartUtc: string, shiftEndUtc: string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(
        `${BASE_URL}/drivers/${driverId}/shift-stats?shiftStartUtc=${encodeURIComponent(shiftStartUtc)}&shiftEndUtc=${encodeURIComponent(shiftEndUtc)}`,
        requestOptions
      );
      if (!response.ok) throw new Error(`Failed to fetch shift stats for driver ${driverId}`);
      return response.json();
    },

    updateStatus: async (driverId: string, statusData: { status: string; currentLat: number; currentLng: number }) => {
      const requestOptions = {
        method: 'PUT',
        headers: getHeaders(true),
        body: JSON.stringify(statusData),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/drivers/${driverId}/status`, requestOptions);
      if (!response.ok) throw new Error(`Failed to update status for driver ${driverId}`);
      return response.json();
    },

    syncOfflineTrips: async (syncData: { driverId: string; trips: OfflineTrip[] }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(syncData),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/drivers/sync-offline`, requestOptions);
      if (!response.ok) throw new Error('Failed to sync offline trips');
      return response.json();
    }
  },

  trips: {
    create: async (payload: CreateTripPayload) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(payload),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips`, requestOptions);
      if (!response.ok) throw new Error('Failed to create a new trip');
      return response.json();
    },

    getById: async (tripId: number | string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/${tripId}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch details for trip ${tripId}`);
      return response.json();
    },

    update: async (tripId: number | string, payload: UpdateTripPayload) => {
      const requestOptions = {
        method: 'PUT',
        headers: getHeaders(true),
        body: JSON.stringify(payload),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/${tripId}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to update trip ${tripId}`);
      return response.json();
    },

    delete: async (tripId: number | string) => {
      const requestOptions = {
        method: 'DELETE',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/${tripId}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to delete trip ${tripId}`);
      return response.text();
    },

    getList: async (pageNumber = 1, pageSize = 10, startDate = "2024-01-01", endDate = "2024-12-31") => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(
        `${BASE_URL}/trips?pageNumber=${pageNumber}&pageSize=${pageSize}&startDate=${startDate}&endDate=${endDate}`,
        requestOptions
      );
      if (!response.ok) throw new Error('Failed to fetch filtered trips list');
      return response.json();
    },

    getHistory: async (pageNumber = 1, pageSize = 10) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/history?pageNumber=${pageNumber}&pageSize=${pageSize}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch trips history');
      return response.json();
    },

    getByZone: async (zoneId: number | string, pageNumber = 1, pageSize = 10) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/zone/${zoneId}?pageNumber=${pageNumber}&pageSize=${pageSize}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch trips for zone ${zoneId}`);
      return response.json();
    },

    testAudit: async () => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/test-audit`, requestOptions);
      if (!response.ok) throw new Error('Audit test endpoint failed');
      return response.json();
    },

    startTrip: async (payload: StartTripPayload) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(payload),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/start`, requestOptions);
      if (!response.ok) throw new Error('Failed to start the trip');
      return response.json();
    },

    endTrip: async (payload: EndTripPayload) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(payload),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/end`, requestOptions);
      if (!response.ok) throw new Error('Failed to end the trip');
      return response.json();
    },

    getStatistics: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/statistics`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch global trips statistics');
      return response.json();
    },

    getRevenueStatistics: async (startDate = "2024-01-01", endDate = "2024-12-31") => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/statistics/revenue?startDate=${startDate}&endDate=${endDate}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch revenue statistics');
      return response.json();
    },

    getDemandStatistics: async (startDate = "2024-01-01", endDate = "2024-12-31") => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/statistics/demand?startDate=${startDate}&endDate=${endDate}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch demand statistics');
      return response.json();
    },

    getZonesStatistics: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/statistics/zones`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch trips zones statistics');
      return response.json();
    },

    getPeakHoursStatistics: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/statistics/peak-hours`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch trips peak hours statistics');
      return response.json();
    },

    getTrendsStatistics: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/statistics/trends`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch trips trends statistics');
      return response.json();
    },

    getDriversStatistics: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/statistics/drivers`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch trips drivers statistics');
      return response.json();
    },

    getOnlineTrips: async (page = 1, limit = 100) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/online?page=${page}&limit=${limit}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch live online trips');
      return response.json();
    },

    getDispatchFeed: async (limit = 20, minutesWindow = 60) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/dispatch/feed?limit=${limit}&minutesWindow=${minutesWindow}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch live dispatch feed');
      return response.json();
    },

    manualDispatch: async (payload: ManualDispatchPayload) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(payload),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/dispatch/manual`, requestOptions);
      if (!response.ok) throw new Error('Manual dispatch request failed');
      return response.json();
    },

    updateDriverTripStatus: async (payload: DriverTripStatusPayload) => {
      const requestOptions = {
        method: 'PATCH',
        headers: getHeaders(true),
        body: JSON.stringify(payload),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/trips/driver/status`, requestOptions);
      if (!response.ok) throw new Error('Failed to update driver trip status');
      return response.json();
    }
  },

  simulation: {
    getStatus: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/status`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch simulation status');
      return response.json();
    },

    start: async (config: {
      durationHours: number;
      speedFactor: number;
      totalDrivers: number;
      zoneCount: number;
      startTime: string;
    }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(config),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/start`, requestOptions);
      if (!response.ok) throw new Error('Failed to start simulation');
      return response.json();
    },

    pause: async () => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/pause`, requestOptions);
      if (!response.ok) throw new Error('Failed to pause simulation');
      return response.text();
    },

    resume: async () => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/resume`, requestOptions);
      if (!response.ok) throw new Error('Failed to resume simulation');
      return response.text();
    },

    stop: async () => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/stop`, requestOptions);
      if (!response.ok) throw new Error('Failed to stop simulation');
      return response.text();
    },

    setSpeed: async (speedFactor: number) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify({ speedFactor }),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/speed`, requestOptions);
      if (!response.ok) throw new Error('Failed to update simulation speed');
      return response.text();
    },

    getPlayback: async (startHour = 0, endHour = 23) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/playback?startHour=${startHour}&endHour=${endHour}`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch simulation playback data');
      return response.json();
    },

    getZones: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/zones`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch zones');
      return response.json();
    },

    getZoneHistory: async (zoneId: number | string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/zones/${zoneId}/history`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch history for zone ${zoneId}`);
      return response.json();
    },

    compareZones: async (zoneA: number | string, zoneB: number | string) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/simulation/zones/compare?zoneA=${zoneA}&zoneB=${zoneB}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to compare zone ${zoneA} with zone ${zoneB}`);
      return response.json();
    }
  },

  ai: {
    predictDemand15Min: async (data: { zoneIds: number[]; targetTime: string; roundToInt: boolean }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(data),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/ai/predict/demand-15min`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch 15min demand prediction');
      return response.json();
    },

    predictDemand6h: async (data: { zoneIds: number[]; targetTime: string }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(data),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/ai/predict/demand-6h`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch 6h demand prediction');
      return response.json();
    },

    predictRevenue: async (data: { zoneIds: number[]; targetTime: string }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(data),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/ai/predict/revenue`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch revenue prediction');
      return response.json();
    },

    predictStockout: async (data: { zoneIds: number[]; targetTime: string }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(data),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/ai/predict/stockout`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch stockout predictions');
      return response.json();
    },

    optimizeRepositioning: async (data: {
      timeWindow: string;
      zoneStates: ZoneStateReposition[];
      constraints: { maxMovesPerVehicle: number; maxRelocationDistanceKm: number };
    }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(data),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/ai/optimize/repositioning`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch repositioning optimization plan');
      return response.json();
    },

    optimizeProfitMaximization: async (data: { zoneStates: ZoneStateProfit[] }) => {
      const requestOptions = {
        method: 'POST',
        headers: getHeaders(true),
        body: JSON.stringify(data),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/ai/optimize/profit-maximization`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch profit maximization plan');
      return response.json();
    }
  },

  analytics: {
    getKpis: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/analytics/kpis`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch analytics KPIs');
      return response.json();
    },

    getDemandVelocity: async (zoneId: number | string, hours = 24) => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/analytics/demand-velocity?zoneId=${zoneId}&hours=${hours}`, requestOptions);
      if (!response.ok) throw new Error(`Failed to fetch demand velocity for zone ${zoneId}`);
      return response.json();
    },

    getThresholds: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/analytics/thresholds`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch system thresholds');
      return response.json();
    },

    updateThresholds: async (thresholdsData: ThresholdsConfig) => {
      const requestOptions = {
        method: 'PUT',
        headers: getHeaders(true),
        body: JSON.stringify(thresholdsData),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/analytics/thresholds`, requestOptions);
      if (!response.ok) throw new Error('Failed to update system thresholds');
      return response.json();
    }
  },

  bi: {
    historicalDataRebuild: async (fromDate = "2024-01-01", toDate = "2024-01-01") => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/BusinessIntelligence/HistoricalDataRebuild?from=${fromDate}&to=${toDate}`, requestOptions);
      if (!response.ok) throw new Error('Failed to rebuild historical data');
      return response.json();
    },

    getFetchPerformance: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/BusinessIntelligence/FetchPerformance`, requestOptions);
      if (!response.ok) throw new Error('Failed to fetch performance metrics');
      return response.json();
    },

    refreshTodayLive: async () => {
      const requestOptions = {
        method: 'GET',
        headers: getHeaders(true),
        redirect: 'follow' as RequestRedirect
      };
      const response = await fetch(`${BASE_URL}/BusinessIntelligence/RefreshTodayLive`, requestOptions);
      if (!response.ok) throw new Error('Failed to refresh today live data');
      return response.json();
    }
  }
};