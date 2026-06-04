using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;

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
            var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var dayEnd = targetDate.AddDays(1);

            _logger.LogInformation("[DailyAggregation] Starting for {Date}", targetDate.ToString("yyyy-MM-dd"));

            try
            {
                var trips = await _context.Trips
                    .AsNoTracking()
                    .Where(t => t.StartedAt >= targetDate && t.StartedAt < dayEnd)
                    .ToListAsync(cancellationToken);

                var completedTrips = trips.Where(t => t.EndedAt != null).ToList();
                var activeDrivers = trips.Where(t => t.DriverId.HasValue).Select(t => t.DriverId!.Value).Distinct().Count();

                var avgMinutes = completedTrips.Any()
                   ? completedTrips.Average(t => ((t.EndedAt - t.StartedAt).Value).TotalMinutes)
                   : 0;

                var totalRevenue = completedTrips.Sum(t => t.FareAmount ?? 0);
                var avgFare = completedTrips.Any() ? totalRevenue / completedTrips.Count : 0;

                var targetDateOnly = DateOnly.FromDateTime(targetDate);

                var existing = await _context.DailyStats
                    .FirstOrDefaultAsync(s => s.Date == targetDateOnly, cancellationToken);

                if (existing != null)
                {
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
                    await _context.DailyStats.AddAsync(new DailyStat
                    {
                        Date = targetDateOnly,
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DailyAggregation] Failed");
                throw;
            }
        }
    }
}