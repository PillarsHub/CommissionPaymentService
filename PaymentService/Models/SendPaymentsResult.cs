namespace PaymentService.Models
{
    
    public class SendPaymentsResult
    {
        public List<SendPayment> Payments { get; set; } = new List<SendPayment>();
    }
}
