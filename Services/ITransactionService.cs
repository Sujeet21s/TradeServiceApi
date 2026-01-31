using TradeServiceApi.Models;

namespace TradeServiceApi.Services
{
    public interface ITransactionService
    {
        Task<Response> ProcessTransactionAsync(TransactionDto dto);
        Task<Response> ProcessPendingTransactionsAsync();
        List<Transaction> GetAllTransactions();        
    }
}
