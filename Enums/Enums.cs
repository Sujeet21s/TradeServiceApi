namespace TradeServiceApi.Enums
{
    public class Enums
    {
        public enum TransactionAction
        {
            INSERT = 1,
            UPDATE = 2,
            CANCEL = 3
        }
        public enum TradeSide
        {
            BUY = 1,
            SELL = 2
        }
        public enum TradeStatus
        {
            ACTIVE = 1,
            CANCELLED = 2
        }
    }
}
