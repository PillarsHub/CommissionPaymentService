using PaymentService.Models;

namespace PaymentService.Interfaces
{
    public interface IPayQuickerService
    {
        public Task<AccessToken> GetAccessTokenAsync(string _clientId, string _clientSecret, PaymentEnvironment environment);
        public Task<List<SendPaymentsResult>> SendPaymentsAsync(string accessToken, string accountingId, PaymentEnvironment environment, SendPaymentRequest sendPaymentRequest);
    }
}
