using System.Text.Json.Serialization;

namespace PaymentService.Models
{
    public class SendPayment
    {
        [JsonPropertyName("fundingAccountPublicId")]
        public string FundingAccountPublicId { get; set; } = string.Empty;

        [JsonPropertyName("monetary")]
        public Monetary Monetary { get; set; } 

        [JsonPropertyName("userCompanyAssignedUniqueKey")]
        public string UserCompanyAssignedUniqueKey { get; set; } = string.Empty;

        [JsonPropertyName("userNotificationEmailAddress")]
        public string UserNotificationEmailAddress { get; set; } = string.Empty;

        [JsonPropertyName("accountingId")]
        public string AccountingId { get; set; } = string.Empty;

        [JsonPropertyName("recipientUserLanguageCode")]
        public string RecipientUserLanguageCode { get; set; } = string.Empty;

        [JsonPropertyName("issuePlasticCard")]
        public bool IssuePlasticCard { get; set; }

        [JsonPropertyName("transactionPublicId")]
        public string TransactionPublicId { get; set; }

        [JsonPropertyName("transactionStatusType")]
        public string TransactionStatusType { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
