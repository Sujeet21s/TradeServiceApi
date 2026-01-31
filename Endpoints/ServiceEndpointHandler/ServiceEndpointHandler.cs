using Microsoft.AspNetCore.Mvc;
using TradeServiceApi.Models;
using TradeServiceApi.Services;

namespace TradeServiceApi.Endpoints.ServiceEndpointHandler
{
    public sealed class ServiceEndpointHandler
    {
        private readonly ILogger<ServiceEndpointHandler> _logger;
        private readonly ITransactionService _transactionService;
        private readonly IPositionService _positionService;
        public ServiceEndpointHandler(ILogger<ServiceEndpointHandler> logger, 
                                    ITransactionService transactionService,
                                    IPositionService positionService)
        {
            _logger = logger;
            _transactionService = transactionService;
            _positionService = positionService;
        }
        public async Task<IResult> HandleTransactionServiceAsync([FromBody] TransactionDto request,
                CancellationToken cts)
        {            
            _logger.LogInformation($"Handling service request {request}");
            var result = await _transactionService.ProcessTransactionAsync(request);
            if (!result.IsSuccess)
            {
                _logger.LogError($"Transaction processing failed: {result.Message}");
                return Results.BadRequest(result);
            }
            return Results.Ok(new {result.Data, result.Message });
        }

        public async Task<IResult> HandlePositionServiceAsync(CancellationToken cts)
        {
            _logger.LogInformation("Handling position service request");
            var positions = _positionService.GetAllPositions();
            return Results.Ok(positions);
        }

        public async Task<IResult> HandleGetAllTransactionAsync(CancellationToken cts)
        {
            var transaction = _transactionService.GetAllTransactions();
            
            return Results.Ok(transaction);
        }

    }
}
