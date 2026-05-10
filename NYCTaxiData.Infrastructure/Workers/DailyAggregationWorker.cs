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
                    await Task.Delay(delay, stoppingToken);
                     
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
                     
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task RunAggregationAsync(CancellationToken cancellationToken)
        { 
            using var scope = _scopeFactory.CreateScope();

            var aggregationService = scope.ServiceProvider
                .GetRequiredService<IDailyAggregationService>();
             
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);

            await aggregationService.AggregateAsync(yesterday, cancellationToken);
        }

        private static TimeSpan CalculateDelayUntilMidnight()
        {
            var now = DateTime.UtcNow;
            var midnight = now.Date.AddDays(1);  
            var delay = midnight - now;
             
            return delay <= TimeSpan.Zero
                ? delay.Add(TimeSpan.FromDays(1))
                : delay;
        }
    }
}
