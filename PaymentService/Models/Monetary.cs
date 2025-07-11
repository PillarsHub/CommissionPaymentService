using System.Text.Json.Serialization;

namespace PaymentService.Models
{
    public class Monetary
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("bank")]
        public string? Bank { get; set; }

        [JsonPropertyName("formattedAmount")]
        public string? FormattedAmount { get; set; }
    }
}
