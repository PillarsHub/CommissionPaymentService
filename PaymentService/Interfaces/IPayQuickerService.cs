using PaymentService.Models;

namespace PaymentService.Interfaces
{
    public interface IPayQuickerService
    {
        public Task<string?> GetAccessTokenAsync(string _clientId, string _clientSecret);
        public Task<List<SendPaymentsResult>> SendPaymentsAsync(string accessToken, PaymentEnvironment environment, SendPaymentRequest sendPaymentRequest);
    }
}
