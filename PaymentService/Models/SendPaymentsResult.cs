using System.Text.Json.Serialization;

namespace PaymentService.Models
{

    public class SendPaymentsResult
    {
        [JsonPropertyName("payments")]
        public List<SendPayment> Payments { get; set; } = new List<SendPayment>();
        public string? FailReason { get; set; } = null;
    }
}
