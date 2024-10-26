using PaymentService.Models;

namespace PaymentService.Interfaces
{
    public interface IBonusRepository
    {
        Task UpdateBatch(string batchId, IEnumerable<ReleaseResult> released);
    }
}
