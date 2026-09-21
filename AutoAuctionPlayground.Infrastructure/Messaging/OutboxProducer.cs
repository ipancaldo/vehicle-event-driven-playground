using AutoAuctionPlayground.Application.Interfaces.Messaging;
using AutoAuctionPlayground.Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoAuctionPlayground.Infrastructure.Messaging
{
    // Dispatches outbox rows to the broker. Safe to run as N horizontally-scaled instances at once:
    // ClaimBatch's SKIP LOCKED claim means concurrent instances never grab the same row, so scaling
    // out is done by running more instances of the process, not more of this class.
    public class OutboxProducer(IServiceScopeFactory scopeFactory, ILogger<OutboxProducer> logger) : BackgroundService
    {
        // These should sit in a IOptions<OutboxProducerOptions> so can be externally modified.
        private const int BatchSize = 50;
        private const int MaxConcurrency = 10;
        private static readonly TimeSpan Lease = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

        private readonly string _workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int claimedCount;
                try
                {
                    claimedCount = await ProcessBatchAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // A poll failing (e.g. transient DB error) must not kill the loop.
                    logger.LogError(ex, "Outbox batch failed; retrying after the poll interval.");
                    claimedCount = 0;
                }

                // A full batch likely means there is more backlog waiting; keep draining it instead
                // of idling for the poll interval.
                if (claimedCount < BatchSize)
                    await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private async Task<int> ProcessBatchAsync(CancellationToken stoppingToken)
        {
            using var scope = scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            var claimed = await unitOfWork.OutboxMessages.ClaimBatch(BatchSize, _workerId, Lease, stoppingToken);
            if (claimed.Count == 0)
                return 0;

            await Parallel.ForEachAsync(
                claimed,
                new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrency, CancellationToken = stoppingToken },
                async (message, token) =>
                {
                    try
                    {
                        await publisher.PublishAsync(message, token);
                        message.MarkAsPublished(DateTime.UtcNow);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        logger.LogError(ex, "Failed to publish outbox message {MessageId}.", message.Id);
                        message.RecordFailure(ex.Message, DateTime.UtcNow);
                    }
                });

            // Single sequential commit after the fan-out: mutating each message's own properties above
            // is thread-safe (no shared state between them), but SaveChanges on the shared DbContext
            // is not, so it runs once, here, after Parallel.ForEachAsync has fully completed.
            await unitOfWork.Commit(stoppingToken);
            return claimed.Count;
        }
    }
}
