using static TradeServiceApi.Enums.Enums;

namespace TradeServiceApi.Models
{
    public class ActiveTrade
    {
        public int TradeID { get; set; }
        public int CurrentVersion { get; set; }
        public string SecurityCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public TradeSide Side { get; set; }
        public TradeStatus Status { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Clone method for creating a copy
        public ActiveTrade Clone()
        {
            return new ActiveTrade
            {
                TradeID = this.TradeID,
                CurrentVersion = this.CurrentVersion,
                SecurityCode = this.SecurityCode,
                Quantity = this.Quantity,
                Side = this.Side,
                Status = this.Status,
                LastModifiedDate = this.LastModifiedDate
            };
        }
    }
}
