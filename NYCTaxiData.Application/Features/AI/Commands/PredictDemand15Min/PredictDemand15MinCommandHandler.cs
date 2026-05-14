using MediatR;
using Microsoft.Extensions.Logging;
using NYCTaxiData.Application.Common;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Domain.DTOs;

namespace NYCTaxiData.Application.Features.AI.Commands.PredictDemand15Min;

public class PredictDemand15MinCommandHandler : IRequestHandler<PredictDemand15MinCommand, Result<BatchPredictionResponse<Demand15MinResult>>>
{
    private readonly IAiPredictionService _aiPredictionService; // نستخدم الـ Interface
    private readonly ILogger<PredictDemand15MinCommandHandler> _logger;

    public PredictDemand15MinCommandHandler(IAiPredictionService aiPredictionService, ILogger<PredictDemand15MinCommandHandler> logger)
    {
        _aiPredictionService = aiPredictionService;
        _logger = logger;
    }

    public async Task<Result<BatchPredictionResponse<Demand15MinResult>>> Handle(PredictDemand15MinCommand request, CancellationToken cancellationToken)
    {
        // ميثود Handle هي اللي الـ MediatR مستنيها عشان الـ Error CS0535 يختفي
        var result = await _aiPredictionService.PredictDemand15MinAsync(request.Zones, true, cancellationToken);

        return Result<BatchPredictionResponse<Demand15MinResult>>.Success(result);
    }
}