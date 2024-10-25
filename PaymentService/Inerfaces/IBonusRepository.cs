using PaymentService.Models;

namespace PaymentService.Inerfaces
{
    public interface IBonusRepository
    {
        Task UpdateBatch(string batchId, IEnumerable<ReleaseResult> released);
    }
}
