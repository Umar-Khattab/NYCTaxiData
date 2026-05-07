using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services
{
    public class DailyAggregationService : IDailyAggregationService
    {
        private readonly TaxiDbContext _context;
        private readonly ILogger<DailyAggregationService> _logger;

        public DailyAggregationService(
            TaxiDbContext context,
            ILogger<DailyAggregationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AggregateAsync(
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[DailyAggregation] Starting aggregation for {Date}",
                date.ToString("yyyy-MM-dd"));

            try
            {
                var dayStart = date.Date;
                var dayEnd = dayStart.AddDays(1);

                // ✅ جيب كل الـ Trips اللي خلصت النهارده
                var trips = await _context.Trips
                    .AsNoTracking()
                    .Where(t => t.StartedAt >= dayStart &&
                                t.StartedAt < dayEnd)
                    .ToListAsync(cancellationToken);

                var completedTrips = trips
                    .Where(t => t.EndedAt != null)
                    .ToList();

                // ✅ عدد السائقين اللي شتغلوا النهارده
                var activeDrivers = trips
                    .Select(t => t.DriverId)
                    .Distinct()
                    .Count();

                // ✅ متوسط وقت الرحلة
                var avgMinutes = completedTrips.Any()
                    ? completedTrips
                        .Where(t => t.StartedAt != null && t.EndedAt != null)
                        .Average(t => (t.EndedAt!.Value - t.StartedAt!.Value).TotalMinutes)
                    : 0;

                var totalRevenue = completedTrips.Sum(t => t.ActualFare ?? 0);
                var avgFare = completedTrips.Any()
                    ? totalRevenue / completedTrips.Count
                    : 0;

                // ✅ شوف لو في Record موجود لليوم ده
                var existing = await _context.DailyStats
                    .FirstOrDefaultAsync(
                        s => s.Date == dayStart,
                        cancellationToken);

                if (existing != null)
                {
                    // Update
                    existing.TotalTrips = trips.Count;
                    existing.TotalRevenue = totalRevenue;
                    existing.ActiveDrivers = activeDrivers;
                    existing.AvgTripMinutes = Math.Round(avgMinutes, 2);
                    existing.AvgFare = Math.Round(avgFare, 2);
                    existing.CompletedTrips = completedTrips.Count;
                    existing.CancelledTrips = trips.Count - completedTrips.Count;
                    existing.ComputedAt = DateTime.UtcNow;
                }
                else
                {
                    // Insert
                    await _context.DailyStats.AddAsync(new DailyStats
                    {
                        Date = dayStart,
                        TotalTrips = trips.Count,
                        TotalRevenue = totalRevenue,
                        ActiveDrivers = activeDrivers,
                        AvgTripMinutes = Math.Round(avgMinutes, 2),
                        AvgFare = Math.Round(avgFare, 2),
                        CompletedTrips = completedTrips.Count,
                        CancelledTrips = trips.Count - completedTrips.Count,
                        ComputedAt = DateTime.UtcNow
                    }, cancellationToken);
                }

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "[DailyAggregation] ✅ Done for {Date} | " +
                    "Trips: {Total} | Revenue: {Revenue} | Drivers: {Drivers}",
                    date.ToString("yyyy-MM-dd"),
                    trips.Count,
                    totalRevenue,
                    activeDrivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[DailyAggregation] ❌ Failed for {Date}",
                    date.ToString("yyyy-MM-dd"));
                throw;
            }
        }
    }
}
