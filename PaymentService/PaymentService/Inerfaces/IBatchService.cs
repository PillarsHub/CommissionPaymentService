using PaymentService.Models;

namespace PaymentService.Inerfaces
{
    public interface IBatchService
    {
        public Task ProcesseBatch(Batch batch);
    }
}
