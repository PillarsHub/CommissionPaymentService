using PaymentService.Models;

namespace PaymentService.Interfaces
{
    public interface IBatchService
    {
        public Task ProcesseBatch(Batch batch, string callbackToken, string pq_clientId, string pq_clientSecret, string pq_fundingAccountPublicId, PaymentEnvironment pq_environment);
    }
}
