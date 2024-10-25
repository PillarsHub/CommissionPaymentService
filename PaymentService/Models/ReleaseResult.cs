namespace PaymentService.Models
{
    public class ReleaseResult
    {
        public string BonusId { get; set; } = string.Empty;
        public string NodeId { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? BatchId { get; set; } = null;
        public decimal Amount { get; set; }
        public long DetailId { get; set; }
        public long PeriodId { get; set; }
        public Status Status { get; set; }
    }
}
