using TradeServiceApi.Models;

namespace TradeServiceApi.Repositories
{
    public interface IInMemoryRepository
    {        
        long AddTransaction(Transaction transaction);
        Transaction? GetTransaction(long transactionId);
        List<Transaction> GetAllTransactions();
        void UpdateTransaction(Transaction transaction);
                
        void AddActiveTrade(ActiveTrade trade);
        ActiveTrade? GetActiveTrade(int tradeId);
        void UpdateActiveTrade(ActiveTrade trade);
        List<ActiveTrade> GetAllActiveTrades();

        
        void AddOrUpdatePosition(string securityCode, long delta);        
        List<Position> GetAllPositions();

        
        void AddPendingTransaction(long transactionId, string reason);
        List<long> GetPendingTransactionIds();
        void RemovePendingTransaction(long transactionId);

        
    }
}
