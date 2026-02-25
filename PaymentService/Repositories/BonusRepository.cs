using PaymentService.Interfaces;
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

        //public async Task UpdateBatch(string token, string batchId, IEnumerable<ReleaseResult> released)
        //{
        //    await _client.Put<object, ReleaseResult[]>($"/api/v1/Batches/{batchId}", released.ToArray(), token);
        //}

        public async Task UpdateBatch(string token, string batchId, IEnumerable<ReleaseResult> released)
        {
            const int maxAttempts = 3;
            var payload = released.ToArray();

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await _client.Put<object, ReleaseResult[]>($"/api/v1/Batches/{batchId}", payload, token);
                    return; // success
                }
                catch (Exception) when (attempt < maxAttempts)
                {
                    // small backoff (optional but strongly recommended)
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt));
                }
            }

            // final attempt (lets exception bubble naturally)
            await _client.Put<object, ReleaseResult[]>($"/api/v1/Batches/{batchId}", payload, token);
        }
    }
}
