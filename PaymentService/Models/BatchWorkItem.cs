namespace PaymentService.Models
{
    public class BatchWorkItem
    {
        public Batch Batch { get; set; } = default!;
        public string CallbackToken { get; set; } = string.Empty;
        public string CallbackTokenExpiration { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string FundingAccountPublicId { get; set; } = string.Empty;
        public PaymentEnvironment Environment { get; set; } = PaymentEnvironment.Sandbox;
    }

    public enum PaymentEnvironment
    {
        Live = 0,
        Sandbox = 1,
    }
}
