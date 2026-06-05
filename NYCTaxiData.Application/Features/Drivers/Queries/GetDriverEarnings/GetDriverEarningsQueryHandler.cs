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
using NYCTaxiData.Application.Common.Plumbing;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverEarnings
{
    public sealed class GetDriverEarningsQueryHandler
    : IRequestHandler<GetDriverEarningsQuery, Result<DriverEarningsDto>>
    {
        private readonly IDbConnection _db;

        public GetDriverEarningsQueryHandler(IDbConnection db) => _db = db;

        // 1?? كلاسات داخلية صريحة لمنع الـ dynamic والـ RuntimeBinderException نهائياً
        private class DbHeaderSummaryResult
        {
            public decimal TotalEarnings { get; set; }
            public long Trips { get; set; }
            public decimal Hours { get; set; }
        }

        private class DbDailyBreakdownResult
        {
            public string Day { get; set; }
            public decimal Amount { get; set; } // استقبال الـ decimal القادم من SUM
        }

        private class DbRecentTripResult
        {
            public string From { get; set; }
            public string To { get; set; }
            public DateTime StartTime { get; set; }
            public decimal DurationMinutes { get; set; }
            public decimal Fare { get; set; }
        }

        public async Task<Result<DriverEarningsDto>> Handle(
            GetDriverEarningsQuery request,
            CancellationToken cancellationToken)
        {
            var (startRange, endRange) = GetRange(request.Period);
            var (prevStart, prevEnd) = GetPreviousRange(request.Period);

            var p = new { DriverId = request.DriverId, StartRange = startRange, EndRange = endRange };
            var prev = new { DriverId = request.DriverId, StartRange = prevStart, EndRange = prevEnd };

            // ? 1. Header Summary Query
            const string headerSql = """
            SELECT
                COALESCE(SUM(fare_amount + COALESCE(tip_amount, 0)), 0.0) AS TotalEarnings,
                COUNT(*) AS Trips,
                COALESCE(SUM(EXTRACT(EPOCH FROM (ended_at - started_at))) / 3600.0, 0.0)
                    + (COUNT(*) * 10.0 / 60.0) AS Hours
            FROM trips
            WHERE driver_id = @DriverId
              AND started_at BETWEEN @StartRange AND @EndRange
            """;

            var header = await _db.QuerySingleOrDefaultAsync<DbHeaderSummaryResult>(headerSql, p);
            var prevHeader = await _db.QuerySingleOrDefaultAsync<DbHeaderSummaryResult>(headerSql, prev);

            double totalEarnings = header != null ? (double)header.TotalEarnings : 0.0;
            int trips = header != null ? (int)header.Trips : 0;
            double hours = header != null ? (double)header.Hours : 0.0;
            double prevEarnings = prevHeader != null ? (double)prevHeader.TotalEarnings : 0.0;

            double avgPerTrip = trips > 0 ? totalEarnings / trips : 0.0;
            double earningsPerHour = hours > 0 ? totalEarnings / hours : 0.0;
            string trend = ComputeTrend(totalEarnings, prevEarnings);

            // ? 2. Daily Breakdown Query
            const string breakdownSql = """
            SELECT
                TO_CHAR(started_at, 'Dy') AS Day,
                COALESCE(SUM(fare_amount + COALESCE(tip_amount, 0)), 0.0) AS Amount
            FROM trips
            WHERE driver_id = @DriverId
              AND started_at BETWEEN @StartRange AND @EndRange
            GROUP BY date_trunc('day', started_at), TO_CHAR(started_at, 'Dy')
            ORDER BY date_trunc('day', started_at) ASC
            """;

            // ??? التعديل الجوهري: القراءة عبر كلاس وسيط ثم الـ Cast لـ double لمنع الـ Exception
            var breakdownRows = await _db.QueryAsync<DbDailyBreakdownResult>(breakdownSql, p);
            var breakdown = breakdownRows.Select(b => new DailyBreakdownDto(
                Day: b.Day,
                Amount: (double)b.Amount
            )).ToList();

            // ? 3. Recent Trips Query
            const string recentSql = """
            SELECT
                pz.zone_name AS "From",
                dz.zone_name AS "To",
                t.started_at AS StartTime,
                EXTRACT(EPOCH FROM (t.ended_at - t.started_at)) / 60.0 AS DurationMinutes,
                (t.fare_amount + COALESCE(t.tip_amount, 0)) AS Fare
            FROM trips t
            JOIN location pl ON t.pickup_location_id  = pl.location_id
            JOIN zones    pz ON pl.zone_id             = pz.zone_id
            JOIN location dl ON t.dropoff_location_id = dl.location_id
            JOIN zones    dz ON dl.zone_id             = dz.zone_id
            WHERE t.driver_id = @DriverId
            ORDER BY t.started_at DESC
            LIMIT 10
            """;

            var recentRows = await _db.QueryAsync<DbRecentTripResult>(recentSql, p);

            var recentTrips = recentRows.Select(r => new RecentTripDto(
                From: r.From,
                To: r.To,
                StartTime: r.StartTime,
                Duration: (int)Math.Round((double)r.DurationMinutes),
                Distance: Math.Round((double)r.DurationMinutes * 0.45, 1),
                Fare: (double)r.Fare
            )).ToList();

            // ? 4. Build DTO
            var dto = new DriverEarningsDto(
                HeaderSummary: new HeaderSummaryDto(
                    TotalEarnings: Math.Round(totalEarnings, 2),
                    Trips: trips,
                    Hours: Math.Round(hours, 1)),

                PerformanceStats: new PerformanceStatsDto(
                    AvgPerTrip: Math.Round(avgPerTrip, 2),
                    EarningsPerHour: Math.Round(earningsPerHour, 2),
                    Trend: trend),

                DailyBreakdown: breakdown,
                RecentTrips: recentTrips);

            Console.WriteLine($"🔍 DEBUG: Ranges: Start={startRange}, End={endRange}");
            return Result<DriverEarningsDto>.Success(dto);
        }

        private static (DateTime Start, DateTime End) GetCurrentWeekRange(DateTime today)
        {
            return (today.AddDays(-90), today.AddDays(1).AddTicks(-1));
        }

        private static (DateTime Start, DateTime End) GetRange(string period)
        {
            var today = DateTime.UtcNow.Date;

            return (period ?? "all").ToLower() switch
            {
                "today" => (today, today.AddDays(1).AddTicks(-1)),
                "week" => (today.AddDays(-7), today.AddDays(1).AddTicks(-1)),
                "month" => (today.AddDays(-30), today.AddDays(1).AddTicks(-1)),
                _ => (new DateTime(2026, 3, 1), today.AddDays(1).AddTicks(-1))  
            };
        }

        private static (DateTime Start, DateTime End) GetPreviousRange(string period)
        {
            var (s, e) = GetRange(period);
            var diff = (e - s).TotalDays + 1;
            return (s.AddDays(-diff), e.AddDays(-diff));
        }

        private static string ComputeTrend(double current, double previous)
        {
            if (previous == 0) return "+0.0%";
            var pct = (current - previous) / previous * 100.0;
            return pct >= 0 ? $"+{pct:F1}%" : $"{pct:F1}%";
        }
    }
}