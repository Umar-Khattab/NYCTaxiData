using MediatR;
using NYCTaxiData.Application.DTOs.DriverAnalytics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NYCTaxiData.Application.Common.Plumping;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverAnalytics
{
    public sealed class GetDriverAnalyticsQueryHandler
    : IRequestHandler<GetDriverAnalyticsQuery, Result<DriverAnalyticsDto>>
    {
        private readonly IDbConnection _db;

        public GetDriverAnalyticsQueryHandler(IDbConnection db) => _db = db;

        // 1️⃣ كلاسات داخلية صريحة لمنع الـ dynamic والـ RuntimeBinderException تماماً
        private class DbSummaryResult
        {
            public decimal TotalEarnings { get; set; }
            public long CompletedTrips { get; set; } // الـ COUNT(*) في PostgreSQL بترجع bigint/long
            public decimal OnlineHours { get; set; }
        }

        private class DbPeakHourResult
        {
            public string TimeSlot { get; set; }
            public long Trips { get; set; }
            public decimal Earnings { get; set; }
        }

        private class DbRouteResult
        {
            public string RouteName { get; set; }
            public long TripsCount { get; set; }
            public decimal Fare { get; set; }
        }

        public async Task<Result<DriverAnalyticsDto>> Handle(
            GetDriverAnalyticsQuery request,
            CancellationToken cancellationToken)
        {
             
            var today = DateTime.UtcNow.Date;

            // 2. حساب كم يوم فات على بداية الأسبوع (بفرض إن الأسبوع يبدأ الإثنين مثلاً DayOfWeek.Monday)
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startRange = today.AddDays(-diff); // تاريخ يوم الإثنين الماضي الساعة 12 بالليل

            // 3. نهاية الأسبوع الحالي (الأحد القادم الساعة 11:59 مساءً)
            var endRange = startRange.AddDays(7).AddTicks(-1);

            // حسابات الأسبوع السابق (Trends) هتمشي أوتوماتيك برضه:
            var prevStart = startRange.AddDays(-7);
            var prevEnd = endRange.AddDays(-7);

            var p = new { DriverId = request.DriverId, StartRange = startRange, EndRange = endRange };

            // ✅ 1. Weekly Summary Query
            const string summarySql = """
            SELECT
                COALESCE(SUM(total_amount), 0.0)                                                AS TotalEarnings,
                COUNT(*)                                                                        AS CompletedTrips,
                COALESCE(SUM(EXTRACT(EPOCH FROM (ended_at - started_at))) / 3600.0, 0.0)
                    + (COUNT(*) * 10.0 / 60.0)                                                  AS OnlineHours
            FROM trips
            WHERE driver_id = @DriverId
              AND started_at BETWEEN @StartRange AND @EndRange
            """;

            // قراءة البيانات بنوع صريح وآمن
            var summary = await _db.QuerySingleOrDefaultAsync<DbSummaryResult>(summarySql, p);

            double totalEarnings = summary != null ? (double)summary.TotalEarnings : 0.0;
            double onlineHours = summary != null ? (double)summary.OnlineHours : 0.0;
            int completedTrips = summary != null ? (int)summary.CompletedTrips : 0;
            double avgPerHour = onlineHours > 0 ? totalEarnings / onlineHours : 0.0;

            // ✅ 2. Trends Query (مقارنة الأسبوع الحالي بالأسبوع السابق)
            var prevP = new { DriverId = request.DriverId, StartRange = prevStart, EndRange = prevEnd };
            var prev = await _db.QuerySingleOrDefaultAsync<DbSummaryResult>(summarySql, prevP);

            double prevEarnings = prev != null ? (double)prev.TotalEarnings : 0.0;
            double prevTrips = prev != null ? (double)prev.CompletedTrips : 0.0;

            string earningsTrend = ComputeTrend(totalEarnings, prevEarnings);
            string tripsTrend = ComputeTrend(completedTrips, prevTrips);

            // ✅ 3. Earnings Trend (7 days)
            const string trendSql = """
            SELECT COALESCE(SUM(total_amount), 0.0) AS Amount
            FROM trips
            WHERE driver_id = @DriverId
              AND started_at BETWEEN @StartRange AND @EndRange
            GROUP BY date_trunc('day', started_at)
            ORDER BY date_trunc('day', started_at) ASC
            """;

            // تحديد النوع decimal هنا لأن الـ SUM بيرجع رقم عشري دقيق من الـ DB ثم تحويله لـ double
            var trendDecimals = await _db.QueryAsync<decimal>(trendSql, p);
            var trend = trendDecimals.Select(amt => (double)amt).ToList();

            // ✅ 4. Peak Hours Query
            const string peakSql = """
            SELECT
                CASE
                    WHEN EXTRACT(HOUR FROM started_at) BETWEEN 6  AND 9  THEN '6 AM - 9 AM'
                    WHEN EXTRACT(HOUR FROM started_at) BETWEEN 16 AND 19 THEN '4 PM - 7 PM'
                    ELSE 'Off-Peak'
                END                               AS TimeSlot,
                COUNT(*)                          AS Trips,
                COALESCE(SUM(total_amount), 0.0)  AS Earnings
            FROM trips
            WHERE driver_id = @DriverId
              AND started_at BETWEEN @StartRange AND @EndRange
            GROUP BY 1
            ORDER BY Earnings DESC
            """;

            var peakRows = await _db.QueryAsync<DbPeakHourResult>(peakSql, p);

            var peakHours = peakRows.Select(r => new PeakHourDto(
                TimeSlot: r.TimeSlot,
                Trips: (int)r.Trips,
                Earnings: (double)r.Earnings,
                Percentage: completedTrips > 0
                    ? Math.Round((double)r.Trips / completedTrips * 100.0, 2)
                    : 0.0
            )).ToList();

            // ✅ 5. Top Routes Query
            const string routesSql = """
            SELECT
                (pz.zone_name || ' -> ' || dz.zone_name)   AS RouteName,
                COUNT(*)                                     AS TripsCount,
                ROUND(AVG(t.fare_amount)::numeric, 2)        AS Fare
            FROM trips t
            JOIN location pl ON t.pickup_location_id  = pl.location_id
            JOIN zones    pz ON pl.zone_id             = pz.zone_id
            JOIN location dl ON t.dropoff_location_id = dl.location_id
            JOIN zones    dz ON dl.zone_id             = dz.zone_id
            WHERE t.driver_id   = @DriverId
              AND t.started_at BETWEEN @StartRange AND @EndRange
            GROUP BY pz.zone_name, dz.zone_name
            ORDER BY TripsCount DESC
            LIMIT 3
            """;

            var routeRows = await _db.QueryAsync<DbRouteResult>(routesSql, p);

            var topRoutes = routeRows.Select(r => new TopRouteDto(
                RouteName: r.RouteName,
                TripsCount: (int)r.TripsCount,
                Fare: (double)r.Fare,
                Status: r.TripsCount > 5 ? "trending" : "stable"
            )).ToList();

            // ✅ 6. Build DTO النهائي
            var dto = new DriverAnalyticsDto(
                WeeklySummary: new WeeklySummaryDto(
                    TotalEarnings: Math.Round(totalEarnings, 2),
                    CompletedTrips: completedTrips,
                    OnlineHours: Math.Round(onlineHours, 1),
                    AvgPerHour: Math.Round(avgPerHour, 2),
                    Trends: new List<string> { earningsTrend, tripsTrend }),
                EarningsTrend: trend,
                PeakHours: peakHours,
                TopRoutes: topRoutes,
                WeeklyGoals: new WeeklyGoalsDto(
                    EarningsGoal: new GoalProgressDto(Math.Round(totalEarnings, 2), 2000.0),
                    TripsGoal: new GoalProgressDto(completedTrips, 50)));

            return Result<DriverAnalyticsDto>.Success(dto);
        }

        private static string ComputeTrend(double current, double previous)
        {
            if (previous == 0) return "+0.0%";
            var pct = (current - previous) / previous * 100.0;
            return pct >= 0 ? $"+{pct:F1}%" : $"{pct:F1}%";
        }
    }
}