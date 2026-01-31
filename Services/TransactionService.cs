using static TradeServiceApi.Enums.Enums;
using TradeServiceApi.Models;
using TradeServiceApi.Repositories;

namespace TradeServiceApi.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IInMemoryRepository _repository;
        private readonly IPositionService _positionService;
        private readonly ILogger<TransactionService> _logger;
        private readonly SemaphoreSlim _mainTransactionSemaphore = new(1, 1);
        private readonly SemaphoreSlim _pendingTransactionSemaphore = new(1, 1);

        public TransactionService(
            IInMemoryRepository repository,
            IPositionService positionService,
            ILogger<TransactionService> logger)
        {
            _repository = repository;
            _positionService = positionService;
            _logger = logger;
        }

        public async Task<Response> ProcessTransactionAsync(TransactionDto dto)
        {
            await _mainTransactionSemaphore.WaitAsync();
            try
            {
                _logger.LogInformation($"Processing transaction: TradeID={dto.TradeID}, Version={dto.Version}, Action={dto.Action}");

                var transaction = MapDtoToTransaction(dto);
                if (transaction == null)
                {
                    return Response.Failure("Invalid transaction data");
                }

                var txnId = _repository.AddTransaction(transaction);
                _logger.LogInformation($"Transaction saved with ID: {txnId}");

                var validationResult = ValidateTransaction(transaction);
                if (!validationResult.CanProcess)
                {
                    _repository.AddPendingTransaction(txnId, validationResult.Reason);
                    return Response.Success($"Transaction queued for processing. Reason: {validationResult.Reason}");
                }

                var processResult = ProcessTransaction(transaction);
                if (!processResult.IsSuccess)
                {
                    return processResult;
                }
                await ProcessPendingTransactionsAsync();


                return Response.Success("Transaction processed successfully", new { TransactionID = txnId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transaction");
                return Response.Failure($"Error: {ex.Message}");
            }
            finally
            {
                _mainTransactionSemaphore.Release();
            }
        }

        public async Task<Response> ProcessPendingTransactionsAsync()
        {
            await _pendingTransactionSemaphore.WaitAsync();
            try
            {
                var pendingIds = _repository.GetPendingTransactionIds();
                if (!pendingIds.Any())
                {
                    return Response.Success("No pending transactions");
                }

                _logger.LogInformation($"Processing {pendingIds.Count} pending transactions");

                var processedCount = 0;
                foreach (var txnId in pendingIds)
                {
                    var transaction = _repository.GetTransaction(txnId);
                    if (transaction == null || transaction.IsProcessed)
                    {
                        _repository.RemovePendingTransaction(txnId);
                        continue;
                    }

                    var validationResult = ValidateTransaction(transaction);
                    if (validationResult.CanProcess)
                    {
                        var processResult = ProcessTransaction(transaction);
                        if (processResult.IsSuccess)
                        {
                            _repository.RemovePendingTransaction(txnId);
                            processedCount++;
                            _logger.LogInformation($"Pending transaction {txnId} processed successfully");
                        }
                    }
                }

                return Response.Success($"Processed {processedCount} pending transactions");
            }
            finally
            {
                _pendingTransactionSemaphore.Release();
            }
        }

        private Response ProcessTransaction(Transaction txn)
        {
            try
            {
                var activeTrade = _repository.GetActiveTrade(txn.TradeID);

                switch (txn.Action)
                {
                    case TransactionAction.INSERT:
                        HandleInsert(txn);
                        break;

                    case TransactionAction.UPDATE:
                        if (activeTrade == null)
                        {
                            return Response.Failure("data does not exist");
                        }
                        HandleUpdate(txn, activeTrade);
                        break;

                    case TransactionAction.CANCEL:
                        if (activeTrade == null)
                        {
                            return Response.Failure("Data does not exist");
                        }
                        HandleCancel(txn, activeTrade);
                        break;

                    default:
                        return Response.Failure("Invalid action");
                }

                // Mark transaction as processed
                txn.IsProcessed = true;
                txn.ProcessedDate = DateTime.UtcNow;
                _repository.UpdateTransaction(txn);

                return Response.Success("Transaction processed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing transaction {txn.TransactionID}");
                return Response.Failure(ex.Message);
            }
        }

        private void HandleInsert(Transaction txn)
        {
            var activeTrade = new ActiveTrade
            {
                TradeID = txn.TradeID,
                CurrentVersion = txn.Version,
                SecurityCode = txn.SecurityCode,
                Quantity = txn.Quantity,
                Side = txn.Side,
                Status = TradeStatus.ACTIVE
            };

            _repository.AddActiveTrade(activeTrade);

            // Update position
            var delta = CalculatePositionDelta(txn.Side, txn.Quantity);
            _positionService.UpdatePosition(txn.SecurityCode, delta);

            _logger.LogInformation($"Position updated: {txn.SecurityCode} by {delta}");
        }

        private void HandleUpdate(Transaction txn, ActiveTrade currentTrade)
        {
            _logger.LogInformation($"UPDATE: TradeID={txn.TradeID}, Old: {currentTrade.SecurityCode}/{currentTrade.Quantity}/{currentTrade.Side}, New: {txn.SecurityCode}/{txn.Quantity}/{txn.Side}");

            // Reverse old position impact
            var oldDelta = CalculatePositionDelta(currentTrade.Side, currentTrade.Quantity);
            _positionService.UpdatePosition(currentTrade.SecurityCode, -oldDelta);
            _logger.LogInformation($"Reversed position: {currentTrade.SecurityCode} by {-oldDelta}");

            // Apply new position impact
            var newDelta = CalculatePositionDelta(txn.Side, txn.Quantity);
            _positionService.UpdatePosition(txn.SecurityCode, newDelta);
            _logger.LogInformation($"Applied position: {txn.SecurityCode} by {newDelta}");

            // Update active trade
            currentTrade.CurrentVersion = txn.Version;
            currentTrade.SecurityCode = txn.SecurityCode;
            currentTrade.Quantity = txn.Quantity;
            currentTrade.Side = txn.Side;
            _repository.UpdateActiveTrade(currentTrade);
        }

        private void HandleCancel(Transaction txn, ActiveTrade currentTrade)
        {
            _logger.LogInformation($"CANCEL: TradeID={txn.TradeID}, Using CURRENT trade data: {currentTrade.SecurityCode}/{currentTrade.Quantity}/{currentTrade.Side}");

            var delta = CalculatePositionDelta(currentTrade.Side, currentTrade.Quantity);
            _positionService.UpdatePosition(currentTrade.SecurityCode, -delta);
            _logger.LogInformation($"Reversed position: {currentTrade.SecurityCode} by {-delta}");

            currentTrade.Status = TradeStatus.CANCELLED;
            currentTrade.CurrentVersion = txn.Version;
            _repository.UpdateActiveTrade(currentTrade);
        }

        private long CalculatePositionDelta(TradeSide side, int quantity)
        {
            return side == TradeSide.BUY ? quantity : -quantity;
        }

        private (bool CanProcess, string Reason) ValidateTransaction(Transaction txn)
        {
            var activeTrade = _repository.GetActiveTrade(txn.TradeID);

            if (txn.Action == TransactionAction.INSERT)
            {
                if (activeTrade != null)
                {
                    return (false, "Trade already exists");
                }
                if (txn.Version != 1)
                {
                    return (false, $"INSERT must have Version=1, got Version={txn.Version}");
                }
                return (true, string.Empty);
            }

            
            if (activeTrade == null)
            {
                return (false, "data does not exist");
            }

            if (activeTrade.Status == TradeStatus.CANCELLED)
            {
                return (false, "already cancelled");
            }

            if (txn.Version != activeTrade.CurrentVersion + 1)
            {
                return (false, $"Expected Version={activeTrade.CurrentVersion + 1}, got Version={txn.Version}");
            }

            return (true, string.Empty);
        }

        private Transaction? MapDtoToTransaction(TransactionDto dto)
        {
            try
            {
                return new Transaction
                {
                    TradeID = dto.TradeID,
                    Version = dto.Version,
                    SecurityCode = dto.SecurityCode,
                    Quantity = dto.Quantity,
                    Action = Enum.Parse<TransactionAction>(dto.Action, true),
                    Side = Enum.Parse<TradeSide>(dto.Side, true),
                    IsProcessed = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping DTO to Transaction");
                return null;
            }
        }

        public List<Transaction> GetAllTransactions()
        {
            return _repository.GetAllTransactions();
        }

    }
}

