using System.Text.Json.Serialization;

namespace PaymentService.Models
{
    public class Balance
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("currency")]
        public string CurrencyCode { get; set; }
        [JsonPropertyName("language")]
        public string LanguageCode { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }
}
