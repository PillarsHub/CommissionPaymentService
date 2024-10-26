using PaymentService.Models;

namespace PaymentService.Interfaces
{
    public interface IPayQuickerService
    {
        public Task<AccessToken?> GetAccessTokenAsync();
        public Task<List<SendPaymentsResult>> SendPaymentsAsync(SendPaymentRequest sendPaymentRequest);
        public Task<List<Monetary>> GetBalanceForUserAsync(string userId);
    }
}
