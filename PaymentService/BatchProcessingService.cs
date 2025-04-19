using PaymentService.Interfaces;
using PaymentService.Models;
using System.Threading.Channels;

namespace PaymentService
{
    public class BatchProcessingService : BackgroundService
    {
        private readonly BatchQueue _batchQueue;
        private readonly IServiceProvider _serviceProvider;

        public BatchProcessingService(BatchQueue batchQueue, IServiceProvider serviceProvider)
        {
            _batchQueue = batchQueue;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reader = _batchQueue.Reader;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await reader.ReadAsync(stoppingToken); // Waits until there is an item or token is cancelled

                    using var scope = _serviceProvider.CreateScope();
                    var batchService = scope.ServiceProvider.GetRequiredService<IBatchService>();

                    await batchService.ProcesseBatch(
                        workItem.Batch,
                        workItem.CallbackToken,
                        workItem.ClientId,
                        workItem.ClientSecret,
                        workItem.FundingAccountPublicId,
                        workItem.Environment);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stoppingToken is triggered — no need to log
                }
                catch (Exception ex)
                {
                    // Log unexpected errors
                    // logger.LogError(ex, "Error processing batch work item");
                }
            }
        }
    }

    public class BatchQueue
    {
        private readonly Channel<BatchWorkItem> _queue = Channel.CreateUnbounded<BatchWorkItem>();

        public void Enqueue(BatchWorkItem item)
        {
            if (!_queue.Writer.TryWrite(item))
            {
                throw new InvalidOperationException("Failed to enqueue batch work item.");
            }
        }

        public ChannelReader<BatchWorkItem> Reader => _queue.Reader;
    }
}
