using System.ComponentModel.DataAnnotations;

namespace TradeServiceApi.Models
{
    public class TransactionDto
    {
        [Required]
        public int TradeID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Version { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty; // INSERT, UPDATE, CANCEL

        [Required]
        public string SecurityCode { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public string Side { get; set; } = string.Empty; // BUY, SELL
    }
}
