using System.Collections.Concurrent;
using TradeServiceApi.Models;

namespace TradeServiceApi.Repositories
{
    public class InMemoryRepository : IInMemoryRepository
    {
        //Create collections
        private readonly ConcurrentDictionary<long, Transaction> _transactions = new();
        private readonly ConcurrentDictionary<int, ActiveTrade> _activeTrades = new();
        private readonly ConcurrentDictionary<string, Position> _positions = new();
        private readonly ConcurrentBag<long> _pendingTransactionIds = new();

        private long _transactionIdCounter = 0;
        private readonly object _lockObj = new();
        
        public long AddTransaction(Transaction transaction)
        {
            lock (_lockObj)
            {
                transaction.TransactionID = ++_transactionIdCounter;
                transaction.CreatedDate = DateTime.UtcNow;
                _transactions[transaction.TransactionID] = transaction;
                return transaction.TransactionID;
            }
        }

        public Transaction? GetTransaction(long transactionId)
        {
            _transactions.TryGetValue(transactionId, out var transaction);
            return transaction;
        }

        public List<Transaction> GetAllTransactions()
        {
            return _transactions.Values.OrderBy(t => t.TransactionID).ToList();
        }

        public void UpdateTransaction(Transaction transaction)
        {
            _transactions[transaction.TransactionID] = transaction;
        }
        
        public void AddActiveTrade(ActiveTrade trade)
        {
            trade.LastModifiedDate = DateTime.UtcNow;
            _activeTrades[trade.TradeID] = trade;
        }

        public ActiveTrade? GetActiveTrade(int tradeId)
        {
            _activeTrades.TryGetValue(tradeId, out var trade);
            return trade?.Clone(); // Return a copy to prevent external modifications
        }

        public void UpdateActiveTrade(ActiveTrade trade)
        {
            trade.LastModifiedDate = DateTime.UtcNow;
            _activeTrades[trade.TradeID] = trade;
        }

        public List<ActiveTrade> GetAllActiveTrades()
        {
            return _activeTrades.Values.ToList();
        }
       
        public void AddOrUpdatePosition(string securityCode, long delta)
        {
            _positions.AddOrUpdate(
                securityCode,
                // Add new position
                new Position
                {
                    SecurityCode = securityCode,
                    NetQuantity = delta,
                    LastUpdatedDate = DateTime.UtcNow
                },
                // Update existing position
                (key, existing) =>
                {
                    existing.NetQuantity += delta;
                    existing.LastUpdatedDate = DateTime.UtcNow;
                    return existing;
                }
            );           
        }

        public List<Position> GetAllPositions()
        {
            return _positions.Values.ToList();
        }

        public void AddPendingTransaction(long transactionId, string reason)
        {
            _pendingTransactionIds.Add(transactionId);
        }

        public List<long> GetPendingTransactionIds()
        {
            return _pendingTransactionIds.ToList();
        }

        public void RemovePendingTransaction(long transactionId)
        {
            var items = _pendingTransactionIds.ToList();
            items.Remove(transactionId);

            // Clear and re-add
            while (_pendingTransactionIds.TryTake(out _)) { }
            foreach (var item in items)
            {
                _pendingTransactionIds.Add(item);
            }
        }        
    }
}
