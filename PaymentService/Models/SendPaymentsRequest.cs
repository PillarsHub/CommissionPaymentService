namespace PaymentService.Models
{
    public class SendPaymentRequest
    {
        public List<SendPayment> Payments { get; set; } = new List<SendPayment>();
    }
}
