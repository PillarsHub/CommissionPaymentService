using PaymentService.Inerfaces;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class BatchService : IBatchService
    {
        private readonly IBonusRepository _bonusRepository;

        public BatchService(IBonusRepository bonusRepository)
        {
            _bonusRepository = bonusRepository;
        }

        public async Task ProcesseBatch(Batch batch)
        {
            //Process the bonuses and mark them released.
            var bonuses = batch.Releases.Select(x => { x.Status = Status.Success; return x; });
            await _bonusRepository.UpdateBatch(batch.Id, bonuses);
        }
    }
}
