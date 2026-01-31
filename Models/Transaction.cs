using static TradeServiceApi.Enums.Enums;

namespace TradeServiceApi.Models
{
    public class Transaction
    {
        public long TransactionID { get; set; }
        public int TradeID { get; set; }
        public int Version { get; set; }
        public string SecurityCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public TransactionAction Action { get; set; }
        public TradeSide Side { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public bool IsProcessed { get; set; }
    }
}
