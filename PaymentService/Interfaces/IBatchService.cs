using PaymentService.Models;

namespace PaymentService.Interfaces
{
    public interface IBatchService
    {
        public Task ProcesseBatch(Batch batch);
    }
}
