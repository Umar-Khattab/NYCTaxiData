using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Workers
{
    public class DailyAggregationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyAggregationWorker> _logger;

        // ✅ وقت التشغيل — الـ Midnight بالظبط
        private static readonly TimeOnly TargetTime = new(0, 0, 0);

        public DailyAggregationWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<DailyAggregationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "[Worker] DailyAggregationWorker started. " +
                "Will run daily at {TargetTime}", TargetTime);

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = CalculateDelayUntilMidnight();

                _logger.LogInformation(
                    "[Worker] Next aggregation in {Hours}h {Minutes}m {Seconds}s",
                    (int)delay.TotalHours,
                    delay.Minutes,
                    delay.Seconds);

                try
                {
                    // ✅ انتظر لحد الـ Midnight
                    await Task.Delay(delay, stoppingToken);

                    // ✅ نفّذ الـ Aggregation
                    await RunAggregationAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation(
                        "[Worker] DailyAggregationWorker stopping...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[Worker] Unexpected error. Retrying in 1 minute.");

                    // ✅ لو في Error، استنى دقيقة وحاول تاني
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task RunAggregationAsync(CancellationToken cancellationToken)
        {
            // ✅ بنعمل Scope جديد لأن الـ DbContext Scoped مش Singleton
            using var scope = _scopeFactory.CreateScope();

            var aggregationService = scope.ServiceProvider
                .GetRequiredService<IDailyAggregationService>();

            // ✅ بنجمع إحصائيات أمس
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);

            await aggregationService.AggregateAsync(yesterday, cancellationToken);
        }

        private static TimeSpan CalculateDelayUntilMidnight()
        {
            var now = DateTime.UtcNow;
            var midnight = now.Date.AddDays(1); // الـ Midnight الجاي
            var delay = midnight - now;

            // ✅ لو الـ Delay أقل من ثانية، استنى لحد الـ Midnight الجاي
            return delay <= TimeSpan.Zero
                ? delay.Add(TimeSpan.FromDays(1))
                : delay;
        }
    }
}
