namespace TradeServiceApi.Models
{
    public class Position
    {
        public string SecurityCode { get; set; } = string.Empty;
        public long NetQuantity { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}
