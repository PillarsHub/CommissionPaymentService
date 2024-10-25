using PaymentService.Inerfaces;
using PaymentService.Models;

namespace PaymentService.Repositories
{
    public class BonusRepository : IBonusRepository
    {
        private readonly IClient _client;

        public BonusRepository(IClient client)
        {
            _client = client;
        }

        public async Task UpdateBatch(string batchId, IEnumerable<ReleaseResult> released)
        {
            await _client.Put<object, ReleaseResult[]>($"/api/v1/Batches/{batchId}", released.ToArray());
        }
    }
}
