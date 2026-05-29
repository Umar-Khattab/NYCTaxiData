using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using NYCTaxiData.Application.Common.Events;

namespace NYCTaxiData.Application.Features.Zones.Events;

/// <summary>
/// Event handler in the Zones sub-domain that asynchronously consumes <see cref="PredictionGeneratedEvent"/> notifications.
/// This enables the Zones module to react to new AI forecasts (e.g. updating heatmaps, KPIs, or live read projections)
/// without being tightly coupled to the AI prediction service or controller flows.
/// </summary>
public class PredictionGeneratedEventHandler : INotificationHandler<PredictionGeneratedEvent>
{
    private readonly ILogger<PredictionGeneratedEventHandler> _logger;

    public PredictionGeneratedEventHandler(ILogger<PredictionGeneratedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task Handle(PredictionGeneratedEvent notification, CancellationToken cancellationToken)
    {
        var count = notification.PredictionType switch
        {
            "Demand15Min" => notification.Demand15MinResults?.Count ?? 0,
            "Demand6h" => notification.Demand6hResults?.Count ?? 0,
            "ETA" => notification.EtaResults?.Count ?? 0,
            "Revenue" => notification.RevenueResults?.Count ?? 0,
            "StockOut" => notification.StockOutResults?.Count ?? 0,
            _ => 0
        };

        _logger.LogInformation("Zones Module: Asynchronously received PredictionGeneratedEvent of type {Type} with {Count} predictions. Updating live demand heatmaps and zone query projections.", 
            notification.PredictionType, 
            count);

        // Volatile in-memory cache update or live heatmap projection refresh logic goes here
        
        return Task.CompletedTask;
    }
}
