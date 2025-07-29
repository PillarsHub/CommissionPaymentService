using System.Text.Json.Serialization;

namespace PaymentService.Models
{
    public class AccessToken
    {
        [JsonPropertyName("access_token")]
        public string Token { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        public string FailReason { get; set; } = string.Empty;
    }
}
