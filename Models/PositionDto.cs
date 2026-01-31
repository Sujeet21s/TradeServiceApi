namespace TradeServiceApi.Models
{
    public class PositionDto
    {
        public string SecurityCode { get; set; } = string.Empty;
        public string NetQuantity { get; set; } = string.Empty;
        public DateTime LastUpdatedDate { get; set; }
    }
}
