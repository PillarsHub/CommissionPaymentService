using PaymentService.Models;

namespace PaymentService.Interfaces
{
    public interface IPayQuickerService
    {
        public Task<string?> GetAccessTokenAsync(string _clientId, string _clientSecret, PaymentEnvironment environment);
        public Task<List<SendPaymentsResult>> SendPaymentsAsync(string accessToken, PaymentEnvironment environment, SendPaymentRequest sendPaymentRequest);
    }
}
