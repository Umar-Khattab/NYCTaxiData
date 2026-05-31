//using MediatR;
//using Microsoft.Extensions.Logging;
//using NYCTaxiData.Application.Common.Models;
//using NYCTaxiData.Application.Common.Exceptions;
//using NYCTaxiData.Application.Common.Interfaces;
//using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
//using NYCTaxiData.Domain.DTOs;

//namespace NYCTaxiData.Application.Features.AI.Queries.GetSimulationResult;

///// <summary>
///// Handler for <see cref="GetSimulationResultQuery"/>.
///// </summary>
//public class GetSimulationResultQueryHandler : IRequestHandler<GetSimulationResultQuery, Result<Common.PaginatedList<SimulationResult>>>
//{
//    private readonly IAiPredictionService _aiPredictionService;
//    private readonly ILogger<GetSimulationResultQueryHandler> _logger;

//    /// <summary>
//    /// Initializes a new instance of the <see cref="GetSimulationResultQueryHandler"/> class.
//    /// </summary>
//    public GetSimulationResultQueryHandler(IAiPredictionService aiPredictionService, ILogger<GetSimulationResultQueryHandler> logger)
//    {
//        _aiPredictionService = aiPredictionService;
//        _logger = logger;
//    }

//    /// <inheritdoc />
//    public async Task<Result<Common.PaginatedList<SimulationResult>>> Handle(GetSimulationResultQuery request, CancellationToken cancellationToken)
//    {
//        var result = await _aiPredictionService.GetSimulationResultAsync(request.SimulationId, cancellationToken);

//        if (result is null)
//        {
//            throw new NotFoundException($"Simulation with ID {request.SimulationId} not found");
//        }

//        var list = new List<SimulationResult> { result };
//        var paginatedList = new Common.PaginatedList<SimulationResult>(list, list.Count, request.PageNumber, request.PageSize);

//        return Result<Common.PaginatedList<SimulationResult>>.Success(paginatedList, "Simulation result retrieved successfully");
//    }
//}
